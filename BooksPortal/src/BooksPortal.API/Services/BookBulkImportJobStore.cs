using System.Collections.Concurrent;
using BooksPortal.Application.Features.BulkImport.DTOs;
using BooksPortal.Application.Features.BulkImport.Interfaces;
using ClosedXML.Excel;

namespace BooksPortal.API.Services;

public sealed class BookBulkImportJobStore
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<Guid, BookBulkImportJobRecord> _jobs = new();

    public BookBulkImportJobStore(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Guid Start(byte[] fileBytes, int userId)
    {
        var id = Guid.NewGuid();
        var job = new BookBulkImportJobRecord
        {
            Id = id,
            Status = "Queued",
            StartedAtUtc = DateTime.UtcNow
        };
        _jobs[id] = job;

        _ = Task.Run(async () =>
        {
            job.Status = "Running";
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IBookBulkImportService>();

                await using var validateStream = new MemoryStream(fileBytes);
                var validation = await service.ValidateAsync(validateStream);
                job.TotalRows = validation.TotalRows;

                var progress = new Progress<int>(processed => job.ProcessedRows = processed);
                await using var commitStream = new MemoryStream(fileBytes);
                var report = await service.CommitAsync(commitStream, userId, progress);
                job.Report = report;
                job.ProcessedRows = report.Rows.Count;
                job.TotalRows = report.TotalRows;
                job.ReportFileName = $"book-bulk-import-report-{id:N}.xlsx";
                job.ReportFileBytes = BuildWorkbook(report);
                job.Status = "Completed";
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.Error = ex.Message;
            }
            finally
            {
                job.CompletedAtUtc = DateTime.UtcNow;
            }
        });

        return id;
    }

    public BookBulkImportJobSnapshot? Get(Guid id)
    {
        if (!_jobs.TryGetValue(id, out var job))
            return null;

        return new BookBulkImportJobSnapshot
        {
            Id = job.Id,
            Status = job.Status,
            Error = job.Error,
            TotalRows = job.TotalRows,
            ProcessedRows = job.ProcessedRows,
            StartedAtUtc = job.StartedAtUtc,
            CompletedAtUtc = job.CompletedAtUtc,
            ReportReady = job.ReportFileBytes is not null
        };
    }

    public (byte[] Bytes, string FileName)? GetReport(Guid id)
    {
        if (!_jobs.TryGetValue(id, out var job) || job.ReportFileBytes is null || string.IsNullOrWhiteSpace(job.ReportFileName))
            return null;

        return (job.ReportFileBytes, job.ReportFileName);
    }

    private static byte[] BuildWorkbook(BulkImportReport report)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Import Results");

        var headers = new[] { "RowNumber", "Key", "Status", "Success", "Note", "Issues" };
        for (var col = 0; col < headers.Length; col++)
        {
            worksheet.Cell(1, col + 1).Value = headers[col];
            worksheet.Cell(1, col + 1).Style.Font.Bold = true;
        }

        var issuesByRow = report.Issues
            .GroupBy(i => i.RowNumber)
            .ToDictionary(
                g => g.Key,
                g => string.Join(" | ", g.Select(i => $"{i.Field}:{i.Code}:{i.Message}")));

        var rowIndex = 2;
        foreach (var row in report.Rows.OrderBy(r => r.RowNumber))
        {
            worksheet.Cell(rowIndex, 1).Value = row.RowNumber;
            worksheet.Cell(rowIndex, 2).Value = row.Key;
            worksheet.Cell(rowIndex, 3).Value = row.Status;
            worksheet.Cell(rowIndex, 4).Value = row.Success;
            worksheet.Cell(rowIndex, 5).Value = row.Note ?? string.Empty;
            worksheet.Cell(rowIndex, 6).Value = issuesByRow.GetValueOrDefault(row.RowNumber, string.Empty);
            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

public sealed class BookBulkImportJobSnapshot
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "Queued";
    public string? Error { get; set; }
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public bool ReportReady { get; set; }
}

internal sealed class BookBulkImportJobRecord
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "Queued";
    public string? Error { get; set; }
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public BulkImportReport? Report { get; set; }
    public byte[]? ReportFileBytes { get; set; }
    public string? ReportFileName { get; set; }
}
