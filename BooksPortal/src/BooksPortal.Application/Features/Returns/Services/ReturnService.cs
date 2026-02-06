using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Returns.DTOs;
using BooksPortal.Application.Features.Returns.Interfaces;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.Returns.Services;

public class ReturnService : IReturnService
{
    private readonly IRepository<ReturnSlip> _slipRepo;
    private readonly IRepository<Book> _bookRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReferenceNumberService _refService;

    public ReturnService(
        IRepository<ReturnSlip> slipRepo,
        IRepository<Book> bookRepo,
        IUnitOfWork unitOfWork,
        IReferenceNumberService refService)
    {
        _slipRepo = slipRepo;
        _bookRepo = bookRepo;
        _unitOfWork = unitOfWork;
        _refService = refService;
    }

    public async Task<PaginatedList<ReturnSlipResponse>> GetPagedAsync(int pageNumber, int pageSize, int? academicYearId = null, int? studentId = null)
    {
        var query = _slipRepo.Query()
            .Include(r => r.AcademicYear)
            .Include(r => r.Student)
            .Include(r => r.Items).ThenInclude(i => i.Book)
            .AsQueryable();

        if (academicYearId.HasValue)
            query = query.Where(r => r.AcademicYearId == academicYearId.Value);

        if (studentId.HasValue)
            query = query.Where(r => r.StudentId == studentId.Value);

        var projected = query.OrderByDescending(r => r.ReceivedAt).Select(r => new ReturnSlipResponse
        {
            Id = r.Id,
            ReferenceNo = r.ReferenceNo,
            AcademicYearId = r.AcademicYearId,
            AcademicYearName = r.AcademicYear.Name,
            StudentId = r.StudentId,
            StudentName = r.Student.FullName,
            StudentIndexNo = r.Student.IndexNo,
            ReturnedById = r.ReturnedById,
            ReceivedById = r.ReceivedById,
            ReceivedAt = r.ReceivedAt,
            Notes = r.Notes,
            Items = r.Items.Select(i => new ReturnSlipItemResponse
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book.Title,
                BookCode = i.Book.Code,
                Quantity = i.Quantity,
                Condition = i.Condition,
                ConditionNotes = i.ConditionNotes
            }).ToList()
        });

        return await PaginatedList<ReturnSlipResponse>.CreateAsync(projected, pageNumber, pageSize);
    }

    public async Task<ReturnSlipResponse> GetByIdAsync(int id)
    {
        var slip = await _slipRepo.Query()
            .Include(r => r.AcademicYear)
            .Include(r => r.Student)
            .Include(r => r.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(ReturnSlip), id);

        return MapToResponse(slip);
    }

    public async Task<ReturnSlipResponse> GetByReferenceAsync(string referenceNo)
    {
        var slip = await _slipRepo.Query()
            .Include(r => r.AcademicYear)
            .Include(r => r.Student)
            .Include(r => r.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(r => r.ReferenceNo == referenceNo)
            ?? throw new NotFoundException(nameof(ReturnSlip), referenceNo);

        return MapToResponse(slip);
    }

    public async Task<ReturnSlipResponse> CreateAsync(CreateReturnSlipRequest request, int userId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var referenceNo = await _refService.GenerateAsync("RTN");

            var slip = new ReturnSlip
            {
                ReferenceNo = referenceNo,
                AcademicYearId = request.AcademicYearId,
                StudentId = request.StudentId,
                ReturnedById = request.ReturnedById,
                ReceivedById = userId,
                ReceivedAt = DateTime.UtcNow,
                Notes = request.Notes
            };

            foreach (var item in request.Items)
            {
                var book = await _bookRepo.GetByIdAsync(item.BookId)
                    ?? throw new NotFoundException(nameof(Book), item.BookId);

                slip.Items.Add(new ReturnSlipItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    Condition = item.Condition,
                    ConditionNotes = item.ConditionNotes
                });

                ApplyReturnToBook(book, item.Quantity, item.Condition);
                _bookRepo.Update(book);
            }

            _slipRepo.Add(slip);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return await GetByIdAsync(slip.Id);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task CancelAsync(int id)
    {
        var slip = await _slipRepo.Query()
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(ReturnSlip), id);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var item in slip.Items)
            {
                var book = await _bookRepo.GetByIdAsync(item.BookId)
                    ?? throw new NotFoundException(nameof(Book), item.BookId);

                ReverseReturnFromBook(book, item.Quantity, item.Condition);
                _bookRepo.Update(book);
            }

            _slipRepo.SoftDelete(slip);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Applies return effect to book counters based on condition.
    /// Good/Fair/Poor: decrement Distributed (book goes back to available).
    /// Damaged: decrement Distributed, increment Damaged.
    /// Lost: decrement Distributed, increment Lost.
    /// </summary>
    internal static void ApplyReturnToBook(Book book, int quantity, BookCondition condition)
    {
        book.Distributed -= quantity;

        switch (condition)
        {
            case BookCondition.Good:
            case BookCondition.Fair:
            case BookCondition.Poor:
                break;
            case BookCondition.Damaged:
                book.Damaged += quantity;
                break;
            case BookCondition.Lost:
                book.Lost += quantity;
                break;
        }
    }

    /// <summary>
    /// Reverses a return — restores Distributed and undoes condition effects.
    /// </summary>
    internal static void ReverseReturnFromBook(Book book, int quantity, BookCondition condition)
    {
        book.Distributed += quantity;

        switch (condition)
        {
            case BookCondition.Good:
            case BookCondition.Fair:
            case BookCondition.Poor:
                break;
            case BookCondition.Damaged:
                book.Damaged -= quantity;
                break;
            case BookCondition.Lost:
                book.Lost -= quantity;
                break;
        }
    }

    private static ReturnSlipResponse MapToResponse(ReturnSlip slip)
    {
        return new ReturnSlipResponse
        {
            Id = slip.Id,
            ReferenceNo = slip.ReferenceNo,
            AcademicYearId = slip.AcademicYearId,
            AcademicYearName = slip.AcademicYear.Name,
            StudentId = slip.StudentId,
            StudentName = slip.Student.FullName,
            StudentIndexNo = slip.Student.IndexNo,
            ReturnedById = slip.ReturnedById,
            ReceivedById = slip.ReceivedById,
            ReceivedAt = slip.ReceivedAt,
            Notes = slip.Notes,
            Items = slip.Items.Select(i => new ReturnSlipItemResponse
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book.Title,
                BookCode = i.Book.Code,
                Quantity = i.Quantity,
                Condition = i.Condition,
                ConditionNotes = i.ConditionNotes
            }).ToList()
        };
    }
}
