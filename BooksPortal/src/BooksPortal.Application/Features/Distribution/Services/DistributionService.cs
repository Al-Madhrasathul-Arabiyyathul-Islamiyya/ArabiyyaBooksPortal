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
    private readonly IStaffDirectoryService _staffDirectoryService;

    public DistributionService(
        IRepository<DistributionSlip> slipRepo,
        IRepository<Book> bookRepo,
        IUnitOfWork unitOfWork,
        IReferenceNumberService refService,
        IPdfService pdfService,
        ISlipStorageService storageService,
        IStaffDirectoryService staffDirectoryService)
    {
        _slipRepo = slipRepo;
        _bookRepo = bookRepo;
        _unitOfWork = unitOfWork;
        _refService = refService;
        _pdfService = pdfService;
        _storageService = storageService;
        _staffDirectoryService = staffDirectoryService;
    }

    public async Task<PaginatedList<DistributionSlipResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? academicYearId = null,
        int? studentId = null,
        bool includeCancelled = false)
    {
        var query = _slipRepo.Query()
            .Include(d => d.AcademicYear)
            .Include(d => d.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(d => d.Parent)
            .Include(d => d.Items).ThenInclude(i => i.Book)
            .AsQueryable();

        if (academicYearId.HasValue)
            query = query.Where(d => d.AcademicYearId == academicYearId.Value);

        if (studentId.HasValue)
            query = query.Where(d => d.StudentId == studentId.Value);

        if (!includeCancelled)
            query = query.Where(d => d.LifecycleStatus != SlipLifecycleStatus.Cancelled);

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
            StudentClassName = d.Student.ClassSection.Grade.Name + " - " + d.Student.ClassSection.Section,
            StudentNationalId = d.Student.NationalId,
            ParentId = d.ParentId,
            ParentName = d.Parent.FullName,
            ParentNationalId = d.Parent.NationalId,
            ParentPhone = d.Parent.Phone,
            ParentRelationship = d.Parent.Relationship,
            IssuedById = d.IssuedById,
            IssuedAt = d.IssuedAt,
            LifecycleStatus = d.LifecycleStatus,
            FinalizedById = d.FinalizedById,
            FinalizedAt = d.FinalizedAt,
            CancelledById = d.CancelledById,
            CancelledAt = d.CancelledAt,
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

        var page = await PaginatedList<DistributionSlipResponse>.CreateAsync(projected, pageNumber, pageSize);
        await EnrichWithStaffAsync(page.Items);
        return page;
    }

    public async Task<DistributionSlipResponse> GetByIdAsync(int id)
    {
        var slip = await _slipRepo.Query()
            .Include(d => d.AcademicYear)
            .Include(d => d.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(d => d.Parent)
            .Include(d => d.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException(nameof(DistributionSlip), id);

        var response = MapToResponse(slip);
        await EnrichWithStaffAsync([response]);
        return response;
    }

    public async Task<DistributionSlipResponse> GetByReferenceAsync(string referenceNo)
    {
        var slip = await _slipRepo.Query()
            .Include(d => d.AcademicYear)
            .Include(d => d.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(d => d.Parent)
            .Include(d => d.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(d => d.ReferenceNo == referenceNo)
            ?? throw new NotFoundException(nameof(DistributionSlip), referenceNo);

        var response = MapToResponse(slip);
        await EnrichWithStaffAsync([response]);
        return response;
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
                LifecycleStatus = SlipLifecycleStatus.Processing,
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
            await SaveSlipPdfAsync(slip, response);
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
            .Include(d => d.AcademicYear)
            .Include(d => d.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(d => d.Parent)
            .Include(d => d.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException(nameof(DistributionSlip), id);

        if (slip.LifecycleStatus == SlipLifecycleStatus.Cancelled)
            throw new BusinessRuleException("Cancelled distribution slips cannot be finalized.");
        if (slip.LifecycleStatus == SlipLifecycleStatus.Finalized)
            return;

        slip.LifecycleStatus = SlipLifecycleStatus.Finalized;
        slip.FinalizedById = userId;
        slip.FinalizedAt = DateTime.UtcNow;
        _slipRepo.Update(slip);

        var response = MapToResponse(slip);
        await SaveSlipPdfAsync(slip, response);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, int userId)
    {
        var slip = await _slipRepo.Query()
            .Include(d => d.AcademicYear)
            .Include(d => d.Student).ThenInclude(s => s.ClassSection).ThenInclude(cs => cs.Grade)
            .Include(d => d.Parent)
            .Include(d => d.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException(nameof(DistributionSlip), id);

        if (slip.LifecycleStatus == SlipLifecycleStatus.Finalized)
            throw new BusinessRuleException("Finalized distribution slips cannot be cancelled.");
        if (slip.LifecycleStatus == SlipLifecycleStatus.Cancelled)
            return;

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

            slip.LifecycleStatus = SlipLifecycleStatus.Cancelled;
            slip.CancelledAt = DateTime.UtcNow;
            slip.CancelledById = userId;
            _slipRepo.Update(slip);

            var response = MapToResponse(slip);
            await SaveSlipPdfAsync(slip, response);
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
            StudentClassName = $"{slip.Student.ClassSection.Grade.Name} - {slip.Student.ClassSection.Section}",
            StudentNationalId = slip.Student.NationalId,
            ParentId = slip.ParentId,
            ParentName = slip.Parent.FullName,
            ParentNationalId = slip.Parent.NationalId,
            ParentPhone = slip.Parent.Phone,
            ParentRelationship = slip.Parent.Relationship,
            IssuedById = slip.IssuedById,
            IssuedByName = string.Empty,
            IssuedAt = slip.IssuedAt,
            LifecycleStatus = slip.LifecycleStatus,
            FinalizedById = slip.FinalizedById,
            FinalizedAt = slip.FinalizedAt,
            CancelledById = slip.CancelledById,
            CancelledAt = slip.CancelledAt,
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

    private async Task EnrichWithStaffAsync(ICollection<DistributionSlipResponse> slips)
    {
        if (slips.Count == 0)
            return;

        var staffMap = await _staffDirectoryService.GetByIdsAsync(slips.Select(s => s.IssuedById));

        foreach (var slip in slips)
        {
            if (staffMap.TryGetValue(slip.IssuedById, out var staff))
            {
                slip.IssuedByName = staff.DisplayName;
                slip.IssuedByDesignation = staff.Designation;
            }
        }
    }

    private async Task SaveSlipPdfAsync(DistributionSlip slip, DistributionSlipResponse response)
    {
        var pdfBytes = await _pdfService.GenerateDistributionSlipAsync(response);
        slip.PdfFilePath = await _storageService.SaveAsync(
            "Distribution",
            response.AcademicYearName,
            $"{slip.ReferenceNo}-{slip.LifecycleStatus}",
            pdfBytes);
    }
}
