using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Books.DTOs;
using BooksPortal.Application.Features.Books.Validators;
using BooksPortal.Application.Features.BulkImport.DTOs;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.BulkImport.Services;

public class BookBulkImportService : IBookBulkImportService
{
    private static readonly string[] RequiredHeaders =
    [
        "Code",
        "Title",
        "SubjectCode",
        "Publisher",
        "PublishedYear",
        "Quantity"
    ];

    private readonly IRepository<Book> _bookRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<AcademicYear> _academicYearRepository;
    private readonly IRepository<StockEntry> _stockEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookBulkImportService(
        IRepository<Book> bookRepository,
        IRepository<Subject> subjectRepository,
        IRepository<AcademicYear> academicYearRepository,
        IRepository<StockEntry> stockEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _bookRepository = bookRepository;
        _subjectRepository = subjectRepository;
        _academicYearRepository = academicYearRepository;
        _stockEntryRepository = stockEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkImportReport> ValidateAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var parsed = await ParseAsync(stream, cancellationToken);
        return parsed.Report;
    }

    public async Task<BulkImportReport> CommitAsync(
        Stream stream,
        int userId,
        IProgress<int>? processedRowsProgress = null,
        CancellationToken cancellationToken = default)
    {
        var parsed = await ParseAsync(stream, cancellationToken);
        if (parsed.Rows.Count == 0)
            return parsed.Report;

        var rowsByRowNumber = parsed.Report.Rows.ToDictionary(r => r.RowNumber);
        var rowCodes = parsed.Rows.Select(r => r.Request.Code).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existingBooksByCode = await _bookRepository.Query()
            .Where(b => rowCodes.Contains(b.Code))
            .ToDictionaryAsync(b => b.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var processedRows = 0;
        foreach (var row in parsed.Rows)
        {
            var rowResult = rowsByRowNumber[row.RowNumber];
            try
            {
                var isUpdate = existingBooksByCode.TryGetValue(row.Request.Code, out var entity);
                if (entity is null)
                {
                    entity = new Book
                    {
                        ISBN = string.IsNullOrWhiteSpace(row.Request.ISBN) ? null : row.Request.ISBN,
                        Code = row.Request.Code
                    };
                    _bookRepository.Add(entity);
                }

                entity.Title = row.Request.Title;
                entity.Author = string.IsNullOrWhiteSpace(row.Request.Author) ? null : row.Request.Author;
                entity.Edition = string.IsNullOrWhiteSpace(row.Request.Edition) ? null : row.Request.Edition;
                entity.Publisher = row.Request.Publisher;
                entity.PublishedYear = row.Request.PublishedYear;
                entity.SubjectId = row.Request.SubjectId;
                entity.Grade = string.IsNullOrWhiteSpace(row.Request.Grade) ? null : row.Request.Grade;
                entity.TotalStock = row.Quantity;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _stockEntryRepository.Add(new StockEntry
                {
                    BookId = entity.Id,
                    AcademicYearId = row.AcademicYearId,
                    Quantity = row.Quantity,
                    Source = string.IsNullOrWhiteSpace(row.Source) ? "Bulk Import" : row.Source,
                    Notes = string.IsNullOrWhiteSpace(row.Notes) ? null : row.Notes,
                    EnteredById = userId,
                    EnteredAt = DateTime.UtcNow
                });
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                rowResult.Success = true;
                rowResult.Status = isUpdate ? "Updated" : "Inserted";
                rowResult.Note = isUpdate ? "Updated existing book." : "Inserted new book.";

                if (isUpdate)
                    parsed.Report.UpdatedRows++;
                else
                    parsed.Report.InsertedRows++;

                existingBooksByCode[row.Request.Code] = entity;
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
            finally
            {
                processedRows++;
                processedRowsProgress?.Report(processedRows);
            }
        }

        parsed.Report.CanCommit = parsed.Report.Rows.Any(r => r.Success);
        parsed.Report.ValidRows = parsed.Report.Rows.Count(r => r.Success);
        return parsed.Report;
    }

    private async Task<(List<BookImportRow> Rows, BulkImportReport Report)> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var workbook = BulkImportWorksheetReader.OpenWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("Workbook does not contain any worksheets.");

        var headers = BulkImportWorksheetReader.ReadHeaderMap(worksheet);
        var report = new BulkImportReport { Entity = "Book" };

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

        if (!headers.ContainsKey("AcademicYearId") && !headers.ContainsKey("AcademicYear"))
        {
            report.Issues.Add(new BulkImportRowIssue
            {
                RowNumber = 1,
                Field = "AcademicYear",
                Code = "MissingHeader",
                Message = "Required header 'AcademicYear' (or legacy 'AcademicYearId') is missing."
            });
        }

        if (report.Issues.Count > 0)
        {
            report.CanCommit = false;
            return ([], report);
        }

        var lastRow = BulkImportWorksheetReader.LastDataRow(worksheet);
        var rows = new List<BookImportRow>();
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var validator = new CreateBookRequestValidator();

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var code = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Code");
            var title = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Title");
            var subjectCode = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "SubjectCode");
            var isbn = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "ISBN");
            var author = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Author");
            var edition = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Edition");
            var publisher = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Publisher");
            var publishedYearRaw = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "PublishedYear");
            var grade = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Grade");
            var academicYearRaw = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "AcademicYearId");
            if (string.IsNullOrWhiteSpace(academicYearRaw))
                academicYearRaw = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "AcademicYear");
            var quantityRaw = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Quantity");
            var source = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Source");
            var notes = BulkImportWorksheetReader.Cell(worksheet, rowNumber, headers, "Notes");

            if (string.IsNullOrWhiteSpace(code) &&
                string.IsNullOrWhiteSpace(title) &&
                string.IsNullOrWhiteSpace(subjectCode) &&
                string.IsNullOrWhiteSpace(quantityRaw))
                continue;

            var publishedYear = int.TryParse(publishedYearRaw, out var parsedPublishedYear) ? parsedPublishedYear : 0;
            var quantity = int.TryParse(quantityRaw, out var parsedQuantity) ? parsedQuantity : 0;

            var request = new CreateBookRequest
            {
                Code = code,
                Title = title,
                ISBN = string.IsNullOrWhiteSpace(isbn) ? null : isbn,
                Author = string.IsNullOrWhiteSpace(author) ? null : author,
                Edition = string.IsNullOrWhiteSpace(edition) ? null : edition,
                Publisher = string.IsNullOrWhiteSpace(publisher) ? "Other" : publisher,
                PublishedYear = publishedYear,
                SubjectId = 0,
                Grade = string.IsNullOrWhiteSpace(grade) ? null : grade
            };

            if (!seenCodes.Add(request.Code))
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = rowNumber,
                    Field = nameof(CreateBookRequest.Code),
                    Code = "DuplicateInFile",
                    Message = $"Book code '{request.Code}' is duplicated in file."
                });
            }

            rows.Add(new BookImportRow(rowNumber, request, subjectCode, academicYearRaw, quantity, source, notes));
        }

        report.TotalRows = rows.Count;
        var subjectCodes = rows.Select(r => r.SubjectCode).Where(sc => !string.IsNullOrWhiteSpace(sc)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var subjectMap = subjectCodes.Count == 0
            ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            : await _subjectRepository.Query()
                .Where(s => subjectCodes.Contains(s.Code))
                .ToDictionaryAsync(s => s.Code, s => s.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var academicYearTokens = rows
            .Select(r => r.AcademicYearRaw.Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var numericAcademicYearTokens = academicYearTokens
            .Where(token => int.TryParse(token, out _))
            .Select(int.Parse)
            .Distinct()
            .ToList();

        var academicYearRows = academicYearTokens.Count == 0
            ? new List<AcademicYearLookup>()
            : await _academicYearRepository.Query()
                .Where(a =>
                    numericAcademicYearTokens.Contains(a.Id) ||
                    numericAcademicYearTokens.Contains(a.Year) ||
                    academicYearTokens.Contains(a.Name))
                .Select(a => new AcademicYearLookup(a.Id, a.Year, a.Name))
                .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            if (!subjectMap.TryGetValue(row.SubjectCode, out var subjectId))
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = "SubjectCode",
                    Code = "NotFound",
                    Message = $"Subject with code '{row.SubjectCode}' was not found."
                });
            }
            else
            {
                row.Request.SubjectId = subjectId;
            }

            row.AcademicYearId = ResolveAcademicYearId(row.AcademicYearRaw, academicYearRows);
            if (row.AcademicYearId <= 0)
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = "AcademicYear",
                    Code = "NotFound",
                    Message = $"Academic year '{row.AcademicYearRaw}' was not found."
                });
            }

            if (row.Quantity <= 0)
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = "Quantity",
                    Code = "Validation",
                    Message = "Quantity must be a positive integer."
                });
            }

            var validation = validator.Validate(row.Request);
            foreach (var error in validation.Errors)
            {
                report.Issues.Add(new BulkImportRowIssue
                {
                    RowNumber = row.RowNumber,
                    Field = error.PropertyName,
                    Code = "Validation",
                    Message = error.ErrorMessage
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
        report.CanCommit = report.ValidRows > 0;
        report.Rows = rows.Select(r => new BulkImportRowResult
        {
            RowNumber = r.RowNumber,
            Key = r.Request.Code,
            Success = !rowIssueSet.Contains(r.RowNumber),
            Status = rowIssueSet.Contains(r.RowNumber) ? "Failed" : "Valid",
            Note = rowIssueSet.Contains(r.RowNumber) ? "Validation failed" : "Valid"
        }).ToList();

        return (rows.Where(r => !rowIssueSet.Contains(r.RowNumber)).ToList(), report);
    }

    private static int ResolveAcademicYearId(string rawValue, IReadOnlyCollection<AcademicYearLookup> candidates)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return 0;

        if (int.TryParse(rawValue, out var parsed))
        {
            var byId = candidates.FirstOrDefault(c => c.Id == parsed);
            if (byId is not null)
                return byId.Id;

            var byYear = candidates.FirstOrDefault(c => c.Year == parsed);
            return byYear?.Id ?? 0;
        }

        var byName = candidates.FirstOrDefault(c => string.Equals(c.Name, rawValue, StringComparison.OrdinalIgnoreCase));
        return byName?.Id ?? 0;
    }

    private sealed class BookImportRow
    {
        public BookImportRow(
            int rowNumber,
            CreateBookRequest request,
            string subjectCode,
            string academicYearRaw,
            int quantity,
            string source,
            string notes)
        {
            RowNumber = rowNumber;
            Request = request;
            SubjectCode = subjectCode;
            AcademicYearRaw = academicYearRaw;
            Quantity = quantity;
            Source = source;
            Notes = notes;
        }

        public int RowNumber { get; }
        public CreateBookRequest Request { get; }
        public string SubjectCode { get; }
        public string AcademicYearRaw { get; }
        public int AcademicYearId { get; set; }
        public int Quantity { get; }
        public string Source { get; }
        public string Notes { get; }
    }

    private sealed record AcademicYearLookup(int Id, int Year, string Name);
}
