using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.BulkImport.DTOs;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Validators;
using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.BulkImport.Services;

public class TeacherBulkImportService : ITeacherBulkImportService
{
    private static readonly string[] RequiredHeaders = ["FullName", "NationalId", "Email", "Phone"];

    private readonly IRepository<Teacher> _teacherRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TeacherBulkImportService(
        IRepository<Teacher> teacherRepository,
        IUnitOfWork unitOfWork)
    {
        _teacherRepository = teacherRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkImportReport> ValidateAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var parsed = await ParseAsync(stream, cancellationToken);
        return parsed.Report;
    }

    public async Task<BulkImportReport> CommitAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var parsed = await ParseAsync(stream, cancellationToken);
        if (!parsed.Report.CanCommit)
            return parsed.Report;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var row in parsed.Rows)
            {
                _teacherRepository.Add(new Teacher
                {
                    FullName = row.Request.FullName,
                    NationalId = row.Request.NationalId,
                    Email = string.IsNullOrWhiteSpace(row.Request.Email) ? null : row.Request.Email,
                    Phone = string.IsNullOrWhiteSpace(row.Request.Phone) ? null : row.Request.Phone
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return new BulkImportReport
        {
            Entity = "Teacher",
            TotalRows = parsed.Rows.Count,
            ValidRows = parsed.Rows.Count,
            InvalidRows = 0,
            InsertedRows = parsed.Rows.Count,
            FailedRows = 0,
            CanCommit = true,
            Rows = parsed.Rows.Select(r => new BulkImportRowResult
            {
                RowNumber = r.RowNumber,
                Key = r.Request.NationalId,
                Success = true,
                Note = "Inserted"
            }).ToList()
        };
    }

    private async Task<(List<TeacherImportRow> Rows, BulkImportReport Report)> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var workbook = BulkImportWorksheetReader.OpenWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("Workbook does not contain any worksheets.");

        var headers = BulkImportWorksheetReader.ReadHeaderMap(worksheet);
        var report = new BulkImportReport { Entity = "Teacher" };

        foreach (var header in RequiredHeaders.Where(h => !headers.ContainsKey(h)))
        {
            report.Issues.Add(new BulkImportRowIssue
            {
                RowNumber = 1,
                Field = header,
                Code = "MissingHeader",
                Message = $"Required header '{header}' is missing."
            });
        }

        if (report.Issues.Count > 0)
        {
            report.CanCommit = false;
            return ([], report);
        }

        var lastRow = BulkImportWorksheetReader.LastDataRow(worksheet);
        var rows = new List<TeacherImportRow>();
        var seenNationalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var validator = new CreateTeacherRequestValidator();

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var fullName = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "FullName");
            var nationalId = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "NationalId");
            var email = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Email");
            var phone = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Phone");

            if (string.IsNullOrWhiteSpace(fullName) &&
                string.IsNullOrWhiteSpace(nationalId) &&
                string.IsNullOrWhiteSpace(email) &&
                string.IsNullOrWhiteSpace(phone))
                continue;

            var request = new CreateTeacherRequest
            {
                FullName = fullName,
                NationalId = nationalId,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone
            };

            var validationResult = validator.Validate(request);
            foreach (var error in validationResult.Errors)
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = rowNumber,
                    Field = error.PropertyName,
                    Code = "Validation",
                    Message = error.ErrorMessage
                });
            }

            if (!seenNationalIds.Add(request.NationalId))
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = rowNumber,
                    Field = nameof(CreateTeacherRequest.NationalId),
                    Code = "DuplicateInFile",
                    Message = $"National ID '{request.NationalId}' is duplicated in file."
                });
            }

            rows.Add(new TeacherImportRow(rowNumber, request));
        }

        report.TotalRows = rows.Count;

        var nationalIds = rows.Select(r => r.Request.NationalId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (nationalIds.Count > 0)
        {
            var existingIds = await _teacherRepository.Query()
                .Where(t => nationalIds.Contains(t.NationalId))
                .Select(t => t.NationalId)
                .ToListAsync(cancellationToken);

            var existingSet = existingIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows.Where(r => existingSet.Contains(r.Request.NationalId)))
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = nameof(CreateTeacherRequest.NationalId),
                    Code = "Conflict",
                    Message = $"Teacher with national ID '{row.Request.NationalId}' already exists."
                });
            }
        }

        var rowIssueSet = report.Issues
            .Where(i => i.RowNumber > 1)
            .Select(i => i.RowNumber)
            .ToHashSet();

        report.InvalidRows = rowIssueSet.Count;
        report.ValidRows = Math.Max(0, report.TotalRows - report.InvalidRows);
        report.FailedRows = report.InvalidRows;
        report.CanCommit = report.Issues.Count == 0 && report.TotalRows > 0;
        report.Rows = rows.Select(r => new BulkImportRowResult
        {
            RowNumber = r.RowNumber,
            Key = r.Request.NationalId,
            Success = !rowIssueSet.Contains(r.RowNumber),
            Note = rowIssueSet.Contains(r.RowNumber) ? "Validation failed" : "Valid"
        }).ToList();

        return (rows.Where(r => !rowIssueSet.Contains(r.RowNumber)).ToList(), report);
    }

    private sealed record TeacherImportRow(int RowNumber, CreateTeacherRequest Request);
}
