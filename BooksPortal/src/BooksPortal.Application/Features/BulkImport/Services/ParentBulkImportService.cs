using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.BulkImport.DTOs;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Validators;
using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.BulkImport.Services;

public class ParentBulkImportService : IParentBulkImportService
{
    private static readonly string[] RequiredHeaders = ["FullName", "NationalId"];

    private readonly IRepository<Parent> _parentRepository;
    private readonly IRepository<Student> _studentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ParentBulkImportService(
        IRepository<Parent> parentRepository,
        IRepository<Student> studentRepository,
        IUnitOfWork unitOfWork)
    {
        _parentRepository = parentRepository;
        _studentRepository = studentRepository;
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
                var parent = new Parent
                {
                    FullName = row.Request.FullName,
                    NationalId = row.Request.NationalId,
                    Phone = string.IsNullOrWhiteSpace(row.Request.Phone) ? null : row.Request.Phone,
                    Relationship = string.IsNullOrWhiteSpace(row.Request.Relationship) ? null : row.Request.Relationship
                };

                if (row.StudentId.HasValue)
                {
                    parent.StudentParents.Add(new StudentParent
                    {
                        StudentId = row.StudentId.Value,
                        IsPrimary = true
                    });
                }

                _parentRepository.Add(parent);
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
            Entity = "Parent",
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

    private async Task<(List<ParentImportRow> Rows, BulkImportReport Report)> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var workbook = BulkImportWorksheetReader.OpenWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("Workbook does not contain any worksheets.");

        var headers = BulkImportWorksheetReader.ReadHeaderMap(worksheet);
        var report = new BulkImportReport { Entity = "Parent" };

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
        var rows = new List<ParentImportRow>();
        var seenNationalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var validator = new CreateParentRequestValidator();

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var fullName = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "FullName");
            var nationalId = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "NationalId");
            var phone = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Phone");
            var relationship = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Relationship");
            var studentIndexNo = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "StudentIndexNo");

            if (string.IsNullOrWhiteSpace(fullName) &&
                string.IsNullOrWhiteSpace(nationalId) &&
                string.IsNullOrWhiteSpace(phone) &&
                string.IsNullOrWhiteSpace(relationship) &&
                string.IsNullOrWhiteSpace(studentIndexNo))
                continue;

            var request = new CreateParentRequest
            {
                FullName = fullName,
                NationalId = nationalId,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                Relationship = string.IsNullOrWhiteSpace(relationship) ? null : relationship
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
                    Field = nameof(CreateParentRequest.NationalId),
                    Code = "DuplicateInFile",
                    Message = $"National ID '{request.NationalId}' is duplicated in file."
                });
            }

            rows.Add(new ParentImportRow(rowNumber, request, string.IsNullOrWhiteSpace(studentIndexNo) ? null : studentIndexNo));
        }

        report.TotalRows = rows.Count;

        var nationalIds = rows.Select(r => r.Request.NationalId).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (nationalIds.Count > 0)
        {
            var existingIds = await _parentRepository.Query()
                .Where(p => nationalIds.Contains(p.NationalId))
                .Select(p => p.NationalId)
                .ToListAsync(cancellationToken);

            var existingSet = existingIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows.Where(r => existingSet.Contains(r.Request.NationalId)))
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = nameof(CreateParentRequest.NationalId),
                    Code = "Conflict",
                    Message = $"Parent with national ID '{row.Request.NationalId}' already exists."
                });
            }
        }

        var studentIndexNumbers = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.StudentIndexNo))
            .Select(r => r.StudentIndexNo!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var studentsByIndex = studentIndexNumbers.Count == 0
            ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            : await _studentRepository.Query()
                .Where(s => studentIndexNumbers.Contains(s.IndexNo))
                .ToDictionaryAsync(s => s.IndexNo, s => s.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var row in rows.Where(r => !string.IsNullOrWhiteSpace(r.StudentIndexNo)))
        {
            if (!studentsByIndex.TryGetValue(row.StudentIndexNo!, out var studentId))
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = "StudentIndexNo",
                    Code = "NotFound",
                    Message = $"Student with index number '{row.StudentIndexNo}' was not found."
                });
                continue;
            }

            row.StudentId = studentId;
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

    private sealed class ParentImportRow
    {
        public ParentImportRow(int rowNumber, CreateParentRequest request, string? studentIndexNo)
        {
            RowNumber = rowNumber;
            Request = request;
            StudentIndexNo = studentIndexNo;
        }

        public int RowNumber { get; }
        public CreateParentRequest Request { get; }
        public string? StudentIndexNo { get; }
        public int? StudentId { get; set; }
    }
}
