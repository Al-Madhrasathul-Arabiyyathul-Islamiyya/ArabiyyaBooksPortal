using BooksPortal.Application.Common.Exceptions;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Common.Models;
using BooksPortal.Application.Features.TeacherIssues.DTOs;
using BooksPortal.Application.Features.TeacherIssues.Interfaces;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.TeacherIssues.Services;

public class TeacherIssueService : ITeacherIssueService
{
    private readonly IRepository<TeacherIssue> _issueRepo;
    private readonly IRepository<TeacherReturnSlip> _returnSlipRepo;
    private readonly IRepository<Book> _bookRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReferenceNumberService _refService;
    private readonly IPdfService _pdfService;
    private readonly ISlipStorageService _storageService;

    public TeacherIssueService(
        IRepository<TeacherIssue> issueRepo,
        IRepository<TeacherReturnSlip> returnSlipRepo,
        IRepository<Book> bookRepo,
        IUnitOfWork unitOfWork,
        IReferenceNumberService refService,
        IPdfService pdfService,
        ISlipStorageService storageService)
    {
        _issueRepo = issueRepo;
        _returnSlipRepo = returnSlipRepo;
        _bookRepo = bookRepo;
        _unitOfWork = unitOfWork;
        _refService = refService;
        _pdfService = pdfService;
        _storageService = storageService;
    }

    public async Task<PaginatedList<TeacherIssueResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        int? academicYearId = null,
        int? teacherId = null,
        bool includeCancelled = false)
    {
        var query = _issueRepo.Query()
            .Include(t => t.AcademicYear)
            .Include(t => t.Teacher)
            .Include(t => t.Items).ThenInclude(i => i.Book)
            .AsQueryable();

        if (academicYearId.HasValue)
            query = query.Where(t => t.AcademicYearId == academicYearId.Value);

        if (teacherId.HasValue)
            query = query.Where(t => t.TeacherId == teacherId.Value);

        if (!includeCancelled)
            query = query.Where(t => t.LifecycleStatus != SlipLifecycleStatus.Cancelled);

        var projected = query.OrderByDescending(t => t.IssuedAt).Select(t => new TeacherIssueResponse
        {
            Id = t.Id,
            ReferenceNo = t.ReferenceNo,
            AcademicYearId = t.AcademicYearId,
            AcademicYearName = t.AcademicYear.Name,
            TeacherId = t.TeacherId,
            TeacherName = t.Teacher.FullName,
            IssuedById = t.IssuedById,
            IssuedAt = t.IssuedAt,
            LifecycleStatus = t.LifecycleStatus,
            FinalizedById = t.FinalizedById,
            FinalizedAt = t.FinalizedAt,
            CancelledById = t.CancelledById,
            CancelledAt = t.CancelledAt,
            ExpectedReturnDate = t.ExpectedReturnDate,
            Status = t.Status,
            Notes = t.Notes,
            PdfFilePath = t.PdfFilePath,
            Items = t.Items.Select(i => new TeacherIssueItemResponse
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book.Title,
                BookCode = i.Book.Code,
                Quantity = i.Quantity,
                ReturnedQuantity = i.ReturnedQuantity,
                OutstandingQuantity = i.Quantity - i.ReturnedQuantity,
                ReturnedAt = i.ReturnedAt,
                ReceivedById = i.ReceivedById
            }).ToList()
        });

        return await PaginatedList<TeacherIssueResponse>.CreateAsync(projected, pageNumber, pageSize);
    }

    public async Task<TeacherIssueResponse> GetByIdAsync(int id)
    {
        var issue = await _issueRepo.Query()
            .Include(t => t.AcademicYear)
            .Include(t => t.Teacher)
            .Include(t => t.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException(nameof(TeacherIssue), id);

        return MapToResponse(issue);
    }

    public async Task<TeacherIssueResponse> CreateAsync(CreateTeacherIssueRequest request, int userId)
    {
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
            var referenceNo = await _refService.GenerateAsync(SlipType.TeacherIssue, request.AcademicYearId);

            var issue = new TeacherIssue
            {
                ReferenceNo = referenceNo,
                AcademicYearId = request.AcademicYearId,
                TeacherId = request.TeacherId,
                IssuedById = userId,
                IssuedAt = DateTime.UtcNow,
                LifecycleStatus = SlipLifecycleStatus.Processing,
                ExpectedReturnDate = request.ExpectedReturnDate,
                Status = TeacherIssueStatus.Active,
                Notes = request.Notes
            };

            foreach (var item in request.Items)
            {
                issue.Items.Add(new TeacherIssueItem
                {
                    BookId = item.BookId,
                    Quantity = item.Quantity,
                    ReturnedQuantity = 0
                });

                var book = await _bookRepo.GetByIdAsync(item.BookId);
                book!.WithTeachers += item.Quantity;
                _bookRepo.Update(book);
            }

            _issueRepo.Add(issue);
            await _unitOfWork.SaveChangesAsync();

            var response = await GetByIdAsync(issue.Id);
            await SaveIssuePdfAsync(issue, response);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            response.PdfFilePath = issue.PdfFilePath;
            return response;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<TeacherIssueResponse> ProcessReturnAsync(int id, ProcessTeacherReturnRequest request, int userId)
    {
        var issue = await _issueRepo.Query()
            .Include(t => t.AcademicYear)
            .Include(t => t.Teacher)
            .Include(t => t.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException(nameof(TeacherIssue), id);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var returnSlipRefNo = await _refService.GenerateAsync(SlipType.TeacherReturn, issue.AcademicYearId);

            var returnSlip = new TeacherReturnSlip
            {
                ReferenceNo = returnSlipRefNo,
                TeacherIssueId = issue.Id,
                ReceivedById = userId,
                ReceivedAt = DateTime.UtcNow,
                Notes = request.Notes
            };

            foreach (var returnItem in request.Items)
            {
                var issueItem = issue.Items.FirstOrDefault(i => i.Id == returnItem.TeacherIssueItemId)
                    ?? throw new NotFoundException(nameof(TeacherIssueItem), returnItem.TeacherIssueItemId);

                if (returnItem.Quantity > issueItem.OutstandingQuantity)
                    throw new BusinessRuleException(
                        $"Cannot return {returnItem.Quantity} — only {issueItem.OutstandingQuantity} outstanding for this item.");

                issueItem.ReturnedQuantity += returnItem.Quantity;
                issueItem.ReturnedAt = DateTime.UtcNow;
                issueItem.ReceivedById = userId;

                var book = await _bookRepo.GetByIdAsync(issueItem.BookId)
                    ?? throw new NotFoundException(nameof(Book), issueItem.BookId);

                book.WithTeachers -= returnItem.Quantity;
                _bookRepo.Update(book);

                returnSlip.Items.Add(new TeacherReturnSlipItem
                {
                    TeacherIssueItemId = issueItem.Id,
                    BookId = issueItem.BookId,
                    Quantity = returnItem.Quantity
                });
            }

            _returnSlipRepo.Add(returnSlip);

            issue.Status = DetermineStatus(issue);
            _issueRepo.Update(issue);
            await _unitOfWork.SaveChangesAsync();

            // Generate and store teacher return PDF
            var returnResponse = new TeacherReturnSlipResponse
            {
                Id = returnSlip.Id,
                ReferenceNo = returnSlip.ReferenceNo,
                TeacherIssueId = issue.Id,
                TeacherName = issue.Teacher.FullName,
                AcademicYearId = issue.AcademicYearId,
                AcademicYearName = issue.AcademicYear.Name,
                ReceivedById = userId,
                ReceivedAt = returnSlip.ReceivedAt,
                Notes = returnSlip.Notes,
                Items = returnSlip.Items.Select(i =>
                {
                    var book = issue.Items.First(ii => ii.BookId == i.BookId).Book;
                    return new TeacherReturnSlipItemResponse
                    {
                        Id = i.Id,
                        BookId = i.BookId,
                        BookTitle = book.Title,
                        BookCode = book.Code,
                        Quantity = i.Quantity
                    };
                }).ToList()
            };

            var returnPdfBytes = await _pdfService.GenerateTeacherReturnSlipAsync(returnResponse);
            returnSlip.PdfFilePath = await _storageService.SaveAsync("TeacherReturn", issue.AcademicYear.Name, returnSlip.ReferenceNo, returnPdfBytes);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            return await GetByIdAsync(issue.Id);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task FinalizeAsync(int id, int userId)
    {
        var issue = await _issueRepo.Query()
            .Include(t => t.AcademicYear)
            .Include(t => t.Teacher)
            .Include(t => t.Items).ThenInclude(i => i.Book)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException(nameof(TeacherIssue), id);

        if (issue.LifecycleStatus == SlipLifecycleStatus.Cancelled)
            throw new BusinessRuleException("Cancelled teacher issues cannot be finalized.");
        if (issue.LifecycleStatus == SlipLifecycleStatus.Finalized)
            return;

        issue.LifecycleStatus = SlipLifecycleStatus.Finalized;
        issue.FinalizedById = userId;
        issue.FinalizedAt = DateTime.UtcNow;
        _issueRepo.Update(issue);

        var response = MapToResponse(issue);
        await SaveIssuePdfAsync(issue, response);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelAsync(int id, int userId)
    {
        var issue = await _issueRepo.Query()
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException(nameof(TeacherIssue), id);

        if (issue.LifecycleStatus == SlipLifecycleStatus.Finalized)
            throw new BusinessRuleException("Finalized teacher issues cannot be cancelled.");
        if (issue.LifecycleStatus == SlipLifecycleStatus.Cancelled)
            return;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var item in issue.Items)
            {
                var outstanding = item.OutstandingQuantity;
                if (outstanding <= 0) continue;

                var book = await _bookRepo.GetByIdAsync(item.BookId)
                    ?? throw new NotFoundException(nameof(Book), item.BookId);

                book.WithTeachers -= outstanding;
                _bookRepo.Update(book);
            }

            issue.LifecycleStatus = SlipLifecycleStatus.Cancelled;
            issue.CancelledAt = DateTime.UtcNow;
            issue.CancelledById = userId;
            _issueRepo.Update(issue);

            var hydratedIssue = await _issueRepo.Query()
                .Include(t => t.AcademicYear)
                .Include(t => t.Teacher)
                .Include(t => t.Items).ThenInclude(i => i.Book)
                .FirstAsync(t => t.Id == id);
            var response = MapToResponse(hydratedIssue);
            await SaveIssuePdfAsync(hydratedIssue, response);

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
    /// Determines the status based on item return progress.
    /// </summary>
    internal static TeacherIssueStatus DetermineStatus(TeacherIssue issue)
    {
        var totalQuantity = issue.Items.Sum(i => i.Quantity);
        var totalReturned = issue.Items.Sum(i => i.ReturnedQuantity);

        if (totalReturned == 0)
            return TeacherIssueStatus.Active;
        if (totalReturned >= totalQuantity)
            return TeacherIssueStatus.Returned;
        return TeacherIssueStatus.Partial;
    }

    private static TeacherIssueResponse MapToResponse(TeacherIssue issue)
    {
        return new TeacherIssueResponse
        {
            Id = issue.Id,
            ReferenceNo = issue.ReferenceNo,
            AcademicYearId = issue.AcademicYearId,
            AcademicYearName = issue.AcademicYear.Name,
            TeacherId = issue.TeacherId,
            TeacherName = issue.Teacher.FullName,
            IssuedById = issue.IssuedById,
            IssuedAt = issue.IssuedAt,
            LifecycleStatus = issue.LifecycleStatus,
            FinalizedById = issue.FinalizedById,
            FinalizedAt = issue.FinalizedAt,
            CancelledById = issue.CancelledById,
            CancelledAt = issue.CancelledAt,
            ExpectedReturnDate = issue.ExpectedReturnDate,
            Status = issue.Status,
            Notes = issue.Notes,
            PdfFilePath = issue.PdfFilePath,
            Items = issue.Items.Select(i => new TeacherIssueItemResponse
            {
                Id = i.Id,
                BookId = i.BookId,
                BookTitle = i.Book.Title,
                BookCode = i.Book.Code,
                Quantity = i.Quantity,
                ReturnedQuantity = i.ReturnedQuantity,
                OutstandingQuantity = i.OutstandingQuantity,
                ReturnedAt = i.ReturnedAt,
                ReceivedById = i.ReceivedById
            }).ToList()
        };
    }

    private async Task SaveIssuePdfAsync(TeacherIssue issue, TeacherIssueResponse response)
    {
        var pdfBytes = await _pdfService.GenerateTeacherIssueSlipAsync(response);
        issue.PdfFilePath = await _storageService.SaveAsync(
            "TeacherIssue",
            response.AcademicYearName,
            $"{issue.ReferenceNo}-{issue.LifecycleStatus}",
            pdfBytes);
    }
}
