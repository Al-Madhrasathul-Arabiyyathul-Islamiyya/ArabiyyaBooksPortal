using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.BulkImport.DTOs;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Validators;
using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.BulkImport.Services;

public class StudentBulkImportService : IStudentBulkImportService
{
    private static readonly string[] RequiredHeaders = ["FullName", "IndexNo", "NationalId", "ClassSectionId"];

    private readonly IRepository<Student> _studentRepository;
    private readonly IRepository<ClassSection> _classSectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StudentBulkImportService(
        IRepository<Student> studentRepository,
        IRepository<ClassSection> classSectionRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _classSectionRepository = classSectionRepository;
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
                _studentRepository.Add(new Student
                {
                    FullName = row.Request.FullName,
                    IndexNo = row.Request.IndexNo,
                    NationalId = row.Request.NationalId,
                    ClassSectionId = row.Request.ClassSectionId
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
            Entity = "Student",
            TotalRows = parsed.Rows.Count,
            ValidRows = parsed.Rows.Count,
            InvalidRows = 0,
            InsertedRows = parsed.Rows.Count,
            FailedRows = 0,
            CanCommit = true,
            Rows = parsed.Rows.Select(r => new BulkImportRowResult
            {
                RowNumber = r.RowNumber,
                Key = r.Request.IndexNo,
                Success = true,
                Note = "Inserted"
            }).ToList()
        };
    }

    private async Task<(List<StudentImportRow> Rows, BulkImportReport Report)> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var workbook = BulkImportWorksheetReader.OpenWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("Workbook does not contain any worksheets.");

        var headers = BulkImportWorksheetReader.ReadHeaderMap(worksheet);
        var report = new BulkImportReport { Entity = "Student" };

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
        var rows = new List<StudentImportRow>();
        var seenIndexNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var validator = new CreateStudentRequestValidator();

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var fullName = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "FullName");
            var indexNo = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "IndexNo");
            var nationalId = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "NationalId");
            var classSectionIdRaw = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "ClassSectionId");

            if (string.IsNullOrWhiteSpace(fullName) &&
                string.IsNullOrWhiteSpace(indexNo) &&
                string.IsNullOrWhiteSpace(nationalId) &&
                string.IsNullOrWhiteSpace(classSectionIdRaw))
                continue;

            var classSectionId = int.TryParse(classSectionIdRaw, out var parsedClassSectionId)
                ? parsedClassSectionId
                : 0;

            var request = new CreateStudentRequest
            {
                FullName = fullName,
                IndexNo = indexNo,
                NationalId = nationalId,
                ClassSectionId = classSectionId
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

            if (!seenIndexNumbers.Add(request.IndexNo))
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = rowNumber,
                    Field = nameof(CreateStudentRequest.IndexNo),
                    Code = "DuplicateInFile",
                    Message = $"Index number '{request.IndexNo}' is duplicated in file."
                });
            }

            rows.Add(new StudentImportRow(rowNumber, request));
        }

        report.TotalRows = rows.Count;

        var indexNos = rows.Select(r => r.Request.IndexNo).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (indexNos.Count > 0)
        {
            var existingIndexNos = await _studentRepository.Query()
                .Where(s => indexNos.Contains(s.IndexNo))
                .Select(s => s.IndexNo)
                .ToListAsync(cancellationToken);

            var existingSet = existingIndexNos.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows.Where(r => existingSet.Contains(r.Request.IndexNo)))
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = nameof(CreateStudentRequest.IndexNo),
                    Code = "Conflict",
                    Message = $"Student with index number '{row.Request.IndexNo}' already exists."
                });
            }
        }

        var classSectionIds = rows
            .Select(r => r.Request.ClassSectionId)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        var existingClassSectionIds = classSectionIds.Count == 0
            ? new List<int>()
            : await _classSectionRepository.Query()
                .Where(cs => classSectionIds.Contains(cs.Id))
                .Select(cs => cs.Id)
                .ToListAsync(cancellationToken);

        var existingClassSectionSet = existingClassSectionIds.ToHashSet();
        foreach (var row in rows.Where(r => r.Request.ClassSectionId > 0 && !existingClassSectionSet.Contains(r.Request.ClassSectionId)))
        {
            report.Issues.Add(new BulkImportRowIssue
            {
                RowNumber = row.RowNumber,
                Field = nameof(CreateStudentRequest.ClassSectionId),
                Code = "NotFound",
                Message = $"Class section with ID '{row.Request.ClassSectionId}' was not found."
            });
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
            Key = r.Request.IndexNo,
            Success = !rowIssueSet.Contains(r.RowNumber),
            Note = rowIssueSet.Contains(r.RowNumber) ? "Validation failed" : "Valid"
        }).ToList();

        return (rows.Where(r => !rowIssueSet.Contains(r.RowNumber)).ToList(), report);
    }

    private sealed record StudentImportRow(int RowNumber, CreateStudentRequest Request);
}
