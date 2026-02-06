using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.Distribution.DTOs;
using BooksPortal.Application.Features.Distribution.Interfaces;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.Distribution.Services;

public class DistributionService : IDistributionService
{
    private readonly IRepository<DistributionSlip> _slipRepo;
    private readonly IRepository<Book> _bookRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReferenceNumberService _refService;
    private readonly IPdfService _pdfService;
    private readonly ISlipStorageService _storageService;

    public DistributionService(
        IRepository<DistributionSlip> slipRepo,
        IRepository<Book> bookRepo,
        IUnitOfWork unitOfWork,
        IReferenceNumberService refService,
        IPdfService pdfService,
        ISlipStorageService storageService)
    {
        _slipRepo = slipRepo;
        _bookRepo = bookRepo;
        _unitOfWork = unitOfWork;
        _refService = refService;
        _pdfService = pdfService;
        _storageService = storageService;
    }

    public async Task<PaginatedList<DistributionSlipResponse>> GetPagedAsync(int pageNumber, int pageSize, int? academicYearId = null, int? studentId = null)
    {
        var query = _slipRepo.Query()
            .Include(d => d.AcademicYear)
            .Include(d => d.Student).ThenInclude(s => s.ClassSection)
            .Include(d => d.Parent)
            .Include(d => d.Items).ThenInclude(i => i.Book)
            .AsQueryable();

        if (academicYearId.HasValue)
            query = query.Where(d => d.AcademicYearId == academicYearId.Value);

        if (studentId.HasValue)
            query = query.Where(d => d.StudentId == studentId.Value);

        var projected = query.OrderByDescending(d => d.IssuedAt).Select(d => new DistributionSlipResponse
        {
            Id = d.Id,
            ReferenceNo = d.ReferenceNo,
            AcademicYearId = d.AcademicYearId,
            AcademicYearName = d.AcademicYear.Name,
            Term = d.Term,
            StudentId = d.StudentId,
            StudentName = d.Student.FullName,
            StudentIndexNo = d.Student.IndexNo,
            StudentClassName = d.Student.ClassSection.Grade + " - " + d.Student.ClassSection.Section,
            StudentNationalId = d.Student.NationalId,
            ParentId = d.ParentId,
            ParentName = d.Parent.FullName,
            IssuedById = d.IssuedById,
            IssuedAt = d.IssuedAt,
            Notes = d.Notes,
            PdfFilePath = d.PdfFilePath,
            Items = d.Items.Select(i => new DistributionSlipItemResponse
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book.Title,
                BookCode = i.Book.Code,
                Quantity = i.Quantity
            }).ToList()
        });

        return await PaginatedList<DistributionSlipResponse>.CreateAsync(projected, pageNumber, pageSize);
    }

    public async Task<DistributionSlipResponse> GetByIdAsync(int id)
    {
        var slip = await _slipRepo.Query()
            .Include(d => d.AcademicYear)
            .Include(d => d.Student).ThenInclude(s => s.ClassSection)
            .Include(d => d.Parent)
            .Include(d => d.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException(nameof(DistributionSlip), id);

        return MapToResponse(slip);
    }

    public async Task<DistributionSlipResponse> GetByReferenceAsync(string referenceNo)
    {
        var slip = await _slipRepo.Query()
            .Include(d => d.AcademicYear)
            .Include(d => d.Student).ThenInclude(s => s.ClassSection)
            .Include(d => d.Parent)
            .Include(d => d.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(d => d.ReferenceNo == referenceNo)
            ?? throw new NotFoundException(nameof(DistributionSlip), referenceNo);

        return MapToResponse(slip);
    }

    public async Task<DistributionSlipResponse> CreateAsync(CreateDistributionSlipRequest request, int userId)
    {
        // Validate books and stock availability
        foreach (var item in request.Items)
        {
            var book = await _bookRepo.GetByIdAsync(item.BookId)
                ?? throw new NotFoundException(nameof(Book), item.BookId);

            var available = book.TotalStock - book.Distributed - book.WithTeachers - book.Damaged - book.Lost;
            if (available < item.Quantity)
                throw new BusinessRuleException($"Insufficient stock for book '{book.Title}'. Available: {available}, Requested: {item.Quantity}.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var referenceNo = await _refService.GenerateAsync(SlipType.Distribution, request.AcademicYearId);

            var slip = new DistributionSlip
            {
                ReferenceNo = referenceNo,
                AcademicYearId = request.AcademicYearId,
                Term = request.Term,
                StudentId = request.StudentId,
                ParentId = request.ParentId,
                IssuedById = userId,
                IssuedAt = DateTime.UtcNow,
                Notes = request.Notes
            };

            foreach (var item in request.Items)
            {
                slip.Items.Add(new DistributionSlipItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity
                });

                // Increment Book.Distributed
                var book = await _bookRepo.GetByIdAsync(item.BookId);
                book!.Distributed += item.Quantity;
                _bookRepo.Update(book);
            }

            _slipRepo.Add(slip);
            await _unitOfWork.SaveChangesAsync();

            var response = await GetByIdAsync(slip.Id);
            var pdfBytes = await _pdfService.GenerateDistributionSlipAsync(response);
            slip.PdfFilePath = await _storageService.SaveAsync("Distribution", response.AcademicYearName, slip.ReferenceNo, pdfBytes);
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

    public async Task CancelAsync(int id)
    {
        var slip = await _slipRepo.Query()
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException(nameof(DistributionSlip), id);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Reverse stock for each item
            foreach (var item in slip.Items)
            {
                var book = await _bookRepo.GetByIdAsync(item.BookId)
                    ?? throw new NotFoundException(nameof(Book), item.BookId);

                book.Distributed -= item.Quantity;
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

    private static DistributionSlipResponse MapToResponse(DistributionSlip slip)
    {
        return new DistributionSlipResponse
        {
            Id = slip.Id,
            ReferenceNo = slip.ReferenceNo,
            AcademicYearId = slip.AcademicYearId,
            AcademicYearName = slip.AcademicYear.Name,
            Term = slip.Term,
            StudentId = slip.StudentId,
            StudentName = slip.Student.FullName,
            StudentIndexNo = slip.Student.IndexNo,
            StudentClassName = $"{slip.Student.ClassSection.Grade} - {slip.Student.ClassSection.Section}",
            StudentNationalId = slip.Student.NationalId,
            ParentId = slip.ParentId,
            ParentName = slip.Parent.FullName,
            IssuedById = slip.IssuedById,
            IssuedAt = slip.IssuedAt,
            Notes = slip.Notes,
            PdfFilePath = slip.PdfFilePath,
            Items = slip.Items.Select(i => new DistributionSlipItemResponse
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book.Title,
                BookCode = i.Book.Code,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}
