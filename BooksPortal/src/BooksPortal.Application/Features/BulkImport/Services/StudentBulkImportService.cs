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
    private readonly IRepository<Parent> _parentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StudentBulkImportService(
        IRepository<Student> studentRepository,
        IRepository<ClassSection> classSectionRepository,
        IRepository<Parent> parentRepository,
        IUnitOfWork unitOfWork)
    {
        _studentRepository = studentRepository;
        _classSectionRepository = classSectionRepository;
        _parentRepository = parentRepository;
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
        if (parsed.Rows.Count == 0)
            return parsed.Report;

        var rowsByRowNumber = parsed.Report.Rows.ToDictionary(r => r.RowNumber);
        var indexNumbers = parsed.Rows.Select(r => r.Request.IndexNo).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existingByIndexNo = await _studentRepository.Query()
            .Include(s => s.StudentParents)
            .Where(s => indexNumbers.Contains(s.IndexNo))
            .ToDictionaryAsync(s => s.IndexNo, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var row in parsed.Rows)
        {
            var rowResult = rowsByRowNumber[row.RowNumber];
            try
            {
                var isUpdate = existingByIndexNo.TryGetValue(row.Request.IndexNo, out var student);
                if (student is null)
                {
                    student = new Student
                    {
                        IndexNo = row.Request.IndexNo
                    };
                    _studentRepository.Add(student);
                }

                student.FullName = row.Request.FullName;
                student.NationalId = row.Request.NationalId;
                student.ClassSectionId = row.Request.ClassSectionId;

                if (row.ParentId.HasValue)
                {
                    student.StudentParents.Clear();
                    student.StudentParents.Add(new StudentParent
                    {
                        ParentId = row.ParentId.Value,
                        IsPrimary = true
                    });
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                rowResult.Success = true;
                rowResult.Status = isUpdate ? "Updated" : "Inserted";
                rowResult.Note = isUpdate ? "Updated existing student." : "Inserted new student.";

                if (isUpdate)
                    parsed.Report.UpdatedRows++;
                else
                    parsed.Report.InsertedRows++;

                existingByIndexNo[row.Request.IndexNo] = student;
            }
            catch (Exception ex)
            {
                rowResult.Success = false;
                rowResult.Status = "Failed";
                rowResult.Note = ex.Message;
                parsed.Report.FailedRows++;
                parsed.Report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = "Commit",
                    Code = "CommitFailed",
                    Message = ex.Message
                });
            }
        }

        parsed.Report.CanCommit = parsed.Report.Rows.Any(r => r.Success);
        parsed.Report.ValidRows = parsed.Report.Rows.Count(r => r.Success);
        return parsed.Report;
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
            var parentNationalId = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "ParentNationalId");

            if (string.IsNullOrWhiteSpace(fullName) &&
                string.IsNullOrWhiteSpace(indexNo) &&
                string.IsNullOrWhiteSpace(nationalId) &&
                string.IsNullOrWhiteSpace(classSectionIdRaw) &&
                string.IsNullOrWhiteSpace(parentNationalId))
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

            rows.Add(new StudentImportRow(rowNumber, request, string.IsNullOrWhiteSpace(parentNationalId) ? null : parentNationalId));
        }

        report.TotalRows = rows.Count;

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

        var parentNationalIds = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.ParentNationalId))
            .Select(r => r.ParentNationalId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var parentsByNationalId = parentNationalIds.Count == 0
            ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            : await _parentRepository.Query()
                .Where(p => parentNationalIds.Contains(p.NationalId))
                .ToDictionaryAsync(p => p.NationalId, p => p.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var row in rows.Where(r => !string.IsNullOrWhiteSpace(r.ParentNationalId)))
        {
            if (!parentsByNationalId.TryGetValue(row.ParentNationalId!, out var parentId))
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = "ParentNationalId",
                    Code = "NotFound",
                    Message = $"Parent with national ID '{row.ParentNationalId}' was not found."
                });
                continue;
            }

            row.ParentId = parentId;
        }

        var rowIssueSet = report.Issues
            .Where(i => i.RowNumber > 1)
            .Select(i => i.RowNumber)
            .ToHashSet();

        report.InvalidRows = rowIssueSet.Count;
        report.ValidRows = Math.Max(0, report.TotalRows - report.InvalidRows);
        report.FailedRows = report.InvalidRows;
        report.CanCommit = report.ValidRows > 0;
        report.Rows = rows.Select(r => new BulkImportRowResult
        {
            RowNumber = r.RowNumber,
            Key = r.Request.IndexNo,
            Success = !rowIssueSet.Contains(r.RowNumber),
            Status = rowIssueSet.Contains(r.RowNumber) ? "Failed" : "Valid",
            Note = rowIssueSet.Contains(r.RowNumber) ? "Validation failed" : "Valid"
        }).ToList();

        return (rows.Where(r => !rowIssueSet.Contains(r.RowNumber)).ToList(), report);
    }

    private sealed class StudentImportRow
    {
        public StudentImportRow(int rowNumber, CreateStudentRequest request, string? parentNationalId)
        {
            RowNumber = rowNumber;
            Request = request;
            ParentNationalId = parentNationalId;
        }

        public int RowNumber { get; }
        public CreateStudentRequest Request { get; }
        public string? ParentNationalId { get; }
        public int? ParentId { get; set; }
    }
}
