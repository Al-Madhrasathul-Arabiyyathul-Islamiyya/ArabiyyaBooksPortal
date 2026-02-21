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
    private readonly IRepository<Parent> _parentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReferenceNumberService _refService;
    private readonly IPdfService _pdfService;
    private readonly ISlipStorageService _storageService;
    private readonly IStaffDirectoryService _staffDirectoryService;

    public ReturnService(
        IRepository<ReturnSlip> slipRepo,
        IRepository<Book> bookRepo,
        IRepository<Parent> parentRepo,
        IUnitOfWork unitOfWork,
        IReferenceNumberService refService,
        IPdfService pdfService,
        ISlipStorageService storageService,
        IStaffDirectoryService staffDirectoryService)
    {
        _slipRepo = slipRepo;
        _bookRepo = bookRepo;
        _parentRepo = parentRepo;
        _unitOfWork = unitOfWork;
        _refService = refService;
        _pdfService = pdfService;
        _storageService = storageService;
        _staffDirectoryService = staffDirectoryService;
    }

    public async Task<PaginatedList<ReturnSlipResponse>> GetPagedAsync(int pageNumber, int pageSize, int? academicYearId = null, int? studentId = null, bool includeCancelled = false)
    {
        var query = _slipRepo.Query()
            .Include(r => r.AcademicYear)
            .Include(r => r.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(r => r.Items).ThenInclude(i => i.Book)
            .AsQueryable();

        if (academicYearId.HasValue)
            query = query.Where(r => r.AcademicYearId == academicYearId.Value);

        if (studentId.HasValue)
            query = query.Where(r => r.StudentId == studentId.Value);

        if (!includeCancelled)
            query = query.Where(r => r.LifecycleStatus != SlipLifecycleStatus.Cancelled);

        var projected = query.OrderByDescending(r => r.ReceivedAt).Select(r => new ReturnSlipResponse
        {
            Id = r.Id,
            ReferenceNo = r.ReferenceNo,
            AcademicYearId = r.AcademicYearId,
            AcademicYearName = r.AcademicYear.Name,
            StudentId = r.StudentId,
            StudentName = r.Student.FullName,
            StudentIndexNo = r.Student.IndexNo,
            StudentClassName = r.Student.ClassSection.Grade.Name + " - " + r.Student.ClassSection.Section,
            StudentNationalId = r.Student.NationalId,
            ReturnedById = r.ReturnedById,
            ReceivedById = r.ReceivedById,
            ReceivedAt = r.ReceivedAt,
            LifecycleStatus = r.LifecycleStatus,
            FinalizedById = r.FinalizedById,
            FinalizedAt = r.FinalizedAt,
            CancelledById = r.CancelledById,
            CancelledAt = r.CancelledAt,
            Notes = r.Notes,
            PdfFilePath = r.PdfFilePath,
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

        var page = await PaginatedList<ReturnSlipResponse>.CreateAsync(projected, pageNumber, pageSize);
        await EnrichPartyAndStaffAsync(page.Items);
        return page;
    }

    public async Task<ReturnSlipResponse> GetByIdAsync(int id)
    {
        var slip = await _slipRepo.Query()
            .Include(r => r.AcademicYear)
            .Include(r => r.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(r => r.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(ReturnSlip), id);

        var response = MapToResponse(slip);
        await EnrichPartyAndStaffAsync([response]);
        return response;
    }

    public async Task<ReturnSlipResponse> GetByReferenceAsync(string referenceNo)
    {
        var slip = await _slipRepo.Query()
            .Include(r => r.AcademicYear)
            .Include(r => r.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(r => r.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(r => r.ReferenceNo == referenceNo)
            ?? throw new NotFoundException(nameof(ReturnSlip), referenceNo);

        var response = MapToResponse(slip);
        await EnrichPartyAndStaffAsync([response]);
        return response;
    }

    public async Task<ReturnSlipResponse> CreateAsync(CreateReturnSlipRequest request, int userId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var referenceNo = await _refService.GenerateAsync(SlipType.Return, request.AcademicYearId);

            var slip = new ReturnSlip
            {
                ReferenceNo = referenceNo,
                AcademicYearId = request.AcademicYearId,
                StudentId = request.StudentId,
                ReturnedById = request.ReturnedById,
                ReceivedById = userId,
                ReceivedAt = ResolveTimestamp(request.ReceivedDate, request.ReceivedTime),
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

            var response = await GetByIdAsync(slip.Id);
            var pdfBytes = await _pdfService.GenerateReturnSlipAsync(response);
            slip.PdfFilePath = await _storageService.SaveAsync("Return", response.AcademicYearName, $"{slip.ReferenceNo}-{slip.LifecycleStatus}", pdfBytes);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            response.PdfFilePath = slip.PdfFilePath;
            return response;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task FinalizeAsync(int id, int userId)
    {
        var slip = await _slipRepo.Query()
            .Include(r => r.AcademicYear)
            .Include(r => r.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(r => r.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(ReturnSlip), id);

        if (slip.LifecycleStatus == SlipLifecycleStatus.Cancelled)
            throw new BusinessRuleException("Cancelled return slips cannot be finalized.");
        if (slip.LifecycleStatus == SlipLifecycleStatus.Finalized)
            return;

        slip.LifecycleStatus = SlipLifecycleStatus.Finalized;
        slip.FinalizedById = userId;
        slip.FinalizedAt = DateTime.UtcNow;
        _slipRepo.Update(slip);

        var response = MapToResponse(slip);
        await EnrichPartyAndStaffAsync([response]);
        var pdfBytes = await _pdfService.GenerateReturnSlipAsync(response);
        slip.PdfFilePath = await _storageService.SaveAsync("Return", response.AcademicYearName, $"{slip.ReferenceNo}-{slip.LifecycleStatus}", pdfBytes);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, int userId)
    {
        var slip = await _slipRepo.Query()
            .Include(r => r.AcademicYear)
            .Include(r => r.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(r => r.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundException(nameof(ReturnSlip), id);

        if (slip.LifecycleStatus == SlipLifecycleStatus.Finalized)
            throw new BusinessRuleException("Finalized return slips cannot be cancelled.");
        if (slip.LifecycleStatus == SlipLifecycleStatus.Cancelled)
            return;

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

            slip.LifecycleStatus = SlipLifecycleStatus.Cancelled;
            slip.CancelledById = userId;
            slip.CancelledAt = DateTime.UtcNow;
            _slipRepo.Update(slip);

            var response = MapToResponse(slip);
            await EnrichPartyAndStaffAsync([response]);
            var pdfBytes = await _pdfService.GenerateReturnSlipAsync(response);
            slip.PdfFilePath = await _storageService.SaveAsync("Return", response.AcademicYearName, $"{slip.ReferenceNo}-{slip.LifecycleStatus}", pdfBytes);

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
            StudentClassName = $"{slip.Student.ClassSection.Grade.Name} - {slip.Student.ClassSection.Section}",
            StudentNationalId = slip.Student.NationalId,
            ReturnedById = slip.ReturnedById,
            ReturnedByName = string.Empty,
            ReceivedById = slip.ReceivedById,
            ReceivedByName = string.Empty,
            ReceivedAt = slip.ReceivedAt,
            LifecycleStatus = slip.LifecycleStatus,
            FinalizedById = slip.FinalizedById,
            FinalizedAt = slip.FinalizedAt,
            CancelledById = slip.CancelledById,
            CancelledAt = slip.CancelledAt,
            Notes = slip.Notes,
            PdfFilePath = slip.PdfFilePath,
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

    private async Task EnrichPartyAndStaffAsync(ICollection<ReturnSlipResponse> slips)
    {
        if (slips.Count == 0)
            return;

        var parentIds = slips.Select(s => s.ReturnedById).Distinct().ToList();
        var parents = await _parentRepo.Query()
            .Where(p => parentIds.Contains(p.Id))
            .Select(p => new { p.Id, p.FullName, p.NationalId, p.Phone, p.Relationship })
            .ToListAsync();
        var parentMap = parents.ToDictionary(p => p.Id);

        var staffMap = await _staffDirectoryService.GetByIdsAsync(slips.Select(s => s.ReceivedById));

        foreach (var slip in slips)
        {
            if (parentMap.TryGetValue(slip.ReturnedById, out var parent))
            {
                slip.ReturnedByName = parent.FullName;
                slip.ReturnedByNationalId = parent.NationalId;
                slip.ReturnedByPhone = parent.Phone;
                slip.ReturnedByRelationship = parent.Relationship;
            }

            if (staffMap.TryGetValue(slip.ReceivedById, out var staff))
            {
                slip.ReceivedByName = staff.DisplayName;
                slip.ReceivedByDesignation = staff.Designation;
            }
        }
    }

    private static DateTime ResolveTimestamp(DateOnly? date, TimeOnly? time)
    {
        var now = DateTime.UtcNow;
        var resolvedDate = date ?? DateOnly.FromDateTime(now);
        var resolvedTime = time ?? TimeOnly.FromDateTime(now);
        return DateTime.SpecifyKind(resolvedDate.ToDateTime(resolvedTime), DateTimeKind.Utc);
    }
}
