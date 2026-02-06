using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Application.Features.Reports.DTOs;
using BooksPortal.Application.Features.Reports.Interfaces;
using BooksPortal.Domain.Entities;
using BooksPortal.Domain.Enums;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Application.Features.Reports.Services;

public class ReportService : IReportService
{
    private readonly IRepository<Book> _bookRepo;
    private readonly IRepository<DistributionSlip> _distRepo;
    private readonly IRepository<TeacherIssue> _teacherIssueRepo;
    private readonly IRepository<ReturnSlip> _returnRepo;

    public ReportService(
        IRepository<Book> bookRepo,
        IRepository<DistributionSlip> distRepo,
        IRepository<TeacherIssue> teacherIssueRepo,
        IRepository<ReturnSlip> returnRepo)
    {
        _bookRepo = bookRepo;
        _distRepo = distRepo;
        _teacherIssueRepo = teacherIssueRepo;
        _returnRepo = returnRepo;
    }

    public async Task<List<StockSummaryReport>> GetStockSummaryAsync(int? subjectId = null, string? grade = null)
    {
        var query = _bookRepo.Query().Include(b => b.Subject).AsQueryable();

        if (subjectId.HasValue)
            query = query.Where(b => b.SubjectId == subjectId.Value);

        if (!string.IsNullOrWhiteSpace(grade))
            query = query.Where(b => b.Grade == grade);

        return await query.OrderBy(b => b.Title).Select(b => new StockSummaryReport
        {
            BookId = b.Id,
            Code = b.Code,
            Title = b.Title,
            SubjectName = b.Subject.Name,
            Grade = b.Grade,
            TotalStock = b.TotalStock,
            Distributed = b.Distributed,
            WithTeachers = b.WithTeachers,
            Damaged = b.Damaged,
            Lost = b.Lost,
            Available = b.TotalStock - b.Distributed - b.WithTeachers - b.Damaged - b.Lost
        }).ToListAsync();
    }

    public async Task<List<DistributionSummaryReport>> GetDistributionSummaryAsync(int academicYearId, DateTime? from = null, DateTime? to = null)
    {
        var query = _distRepo.Query()
            .Include(d => d.Student)
            .Include(d => d.Parent)
            .Include(d => d.Items)
            .Where(d => d.AcademicYearId == academicYearId);

        if (from.HasValue)
            query = query.Where(d => d.IssuedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(d => d.IssuedAt <= to.Value);

        return await query.OrderByDescending(d => d.IssuedAt).Select(d => new DistributionSummaryReport
        {
            SlipId = d.Id,
            ReferenceNo = d.ReferenceNo,
            StudentName = d.Student.FullName,
            StudentIndexNo = d.Student.IndexNo,
            ParentName = d.Parent.FullName,
            IssuedAt = d.IssuedAt,
            TotalBooks = d.Items.Sum(i => i.Quantity)
        }).ToListAsync();
    }

    public async Task<List<TeacherOutstandingReport>> GetTeacherOutstandingAsync(int? teacherId = null)
    {
        var query = _teacherIssueRepo.Query()
            .Include(t => t.Teacher)
            .Include(t => t.Items).ThenInclude(i => i.Book)
            .Where(t => t.Status != TeacherIssueStatus.Returned);

        if (teacherId.HasValue)
            query = query.Where(t => t.TeacherId == teacherId.Value);

        var issues = await query.OrderByDescending(t => t.IssuedAt).ToListAsync();

        return issues.SelectMany(t => t.Items
            .Where(i => i.Quantity - i.ReturnedQuantity > 0)
            .Select(i => new TeacherOutstandingReport
            {
                IssueId = t.Id,
                ReferenceNo = t.ReferenceNo,
                TeacherName = t.Teacher.FullName,
                BookTitle = i.Book.Title,
                BookCode = i.Book.Code,
                Quantity = i.Quantity,
                ReturnedQuantity = i.ReturnedQuantity,
                Outstanding = i.Quantity - i.ReturnedQuantity,
                Status = t.Status,
                IssuedAt = t.IssuedAt,
                ExpectedReturnDate = t.ExpectedReturnDate
            })).ToList();
    }

    public async Task<List<StudentHistoryReport>> GetStudentHistoryAsync(int studentId)
    {
        var distributions = await _distRepo.Query()
            .Include(d => d.Items).ThenInclude(i => i.Book)
            .Where(d => d.StudentId == studentId)
            .ToListAsync();

        var returns = await _returnRepo.Query()
            .Include(r => r.Items).ThenInclude(i => i.Book)
            .Where(r => r.StudentId == studentId)
            .ToListAsync();

        var history = new List<StudentHistoryReport>();

        foreach (var dist in distributions)
        {
            foreach (var item in dist.Items)
            {
                history.Add(new StudentHistoryReport
                {
                    Type = "Distribution",
                    ReferenceNo = dist.ReferenceNo,
                    Date = dist.IssuedAt,
                    BookTitle = item.Book.Title,
                    BookCode = item.Book.Code,
                    Quantity = item.Quantity
                });
            }
        }

        foreach (var ret in returns)
        {
            foreach (var item in ret.Items)
            {
                history.Add(new StudentHistoryReport
                {
                    Type = "Return",
                    ReferenceNo = ret.ReferenceNo,
                    Date = ret.ReceivedAt,
                    BookTitle = item.Book.Title,
                    BookCode = item.Book.Code,
                    Quantity = item.Quantity,
                    Condition = item.Condition.ToString()
                });
            }
        }

        return history.OrderByDescending(h => h.Date).ToList();
    }

    public async Task<byte[]> ExportStockSummaryAsync(int? subjectId = null, string? grade = null)
    {
        var data = await GetStockSummaryAsync(subjectId, grade);
        return GenerateExcel("Stock Summary", wb =>
        {
            var ws = wb.Worksheets.Add("Stock Summary");
            ws.Cell(1, 1).Value = "Code";
            ws.Cell(1, 2).Value = "Title";
            ws.Cell(1, 3).Value = "Subject";
            ws.Cell(1, 4).Value = "Grade";
            ws.Cell(1, 5).Value = "Total Stock";
            ws.Cell(1, 6).Value = "Distributed";
            ws.Cell(1, 7).Value = "With Teachers";
            ws.Cell(1, 8).Value = "Damaged";
            ws.Cell(1, 9).Value = "Lost";
            ws.Cell(1, 10).Value = "Available";
            StyleHeader(ws, 10);

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                ws.Cell(row, 1).Value = data[i].Code;
                ws.Cell(row, 2).Value = data[i].Title;
                ws.Cell(row, 3).Value = data[i].SubjectName;
                ws.Cell(row, 4).Value = data[i].Grade;
                ws.Cell(row, 5).Value = data[i].TotalStock;
                ws.Cell(row, 6).Value = data[i].Distributed;
                ws.Cell(row, 7).Value = data[i].WithTeachers;
                ws.Cell(row, 8).Value = data[i].Damaged;
                ws.Cell(row, 9).Value = data[i].Lost;
                ws.Cell(row, 10).Value = data[i].Available;
            }

            ws.Columns().AdjustToContents();
        });
    }

    public async Task<byte[]> ExportDistributionSummaryAsync(int academicYearId, DateTime? from = null, DateTime? to = null)
    {
        var data = await GetDistributionSummaryAsync(academicYearId, from, to);
        return GenerateExcel("Distribution Summary", wb =>
        {
            var ws = wb.Worksheets.Add("Distributions");
            ws.Cell(1, 1).Value = "Reference No";
            ws.Cell(1, 2).Value = "Student Name";
            ws.Cell(1, 3).Value = "Index No";
            ws.Cell(1, 4).Value = "Parent Name";
            ws.Cell(1, 5).Value = "Issued At";
            ws.Cell(1, 6).Value = "Total Books";
            StyleHeader(ws, 6);

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                ws.Cell(row, 1).Value = data[i].ReferenceNo;
                ws.Cell(row, 2).Value = data[i].StudentName;
                ws.Cell(row, 3).Value = data[i].StudentIndexNo;
                ws.Cell(row, 4).Value = data[i].ParentName;
                ws.Cell(row, 5).Value = data[i].IssuedAt;
                ws.Cell(row, 6).Value = data[i].TotalBooks;
            }

            ws.Columns().AdjustToContents();
        });
    }

    public async Task<byte[]> ExportTeacherOutstandingAsync(int? teacherId = null)
    {
        var data = await GetTeacherOutstandingAsync(teacherId);
        return GenerateExcel("Teacher Outstanding", wb =>
        {
            var ws = wb.Worksheets.Add("Outstanding");
            ws.Cell(1, 1).Value = "Reference No";
            ws.Cell(1, 2).Value = "Teacher";
            ws.Cell(1, 3).Value = "Book";
            ws.Cell(1, 4).Value = "Code";
            ws.Cell(1, 5).Value = "Issued";
            ws.Cell(1, 6).Value = "Returned";
            ws.Cell(1, 7).Value = "Outstanding";
            ws.Cell(1, 8).Value = "Status";
            ws.Cell(1, 9).Value = "Issued At";
            ws.Cell(1, 10).Value = "Expected Return";
            StyleHeader(ws, 10);

            for (int i = 0; i < data.Count; i++)
            {
                var row = i + 2;
                ws.Cell(row, 1).Value = data[i].ReferenceNo;
                ws.Cell(row, 2).Value = data[i].TeacherName;
                ws.Cell(row, 3).Value = data[i].BookTitle;
                ws.Cell(row, 4).Value = data[i].BookCode;
                ws.Cell(row, 5).Value = data[i].Quantity;
                ws.Cell(row, 6).Value = data[i].ReturnedQuantity;
                ws.Cell(row, 7).Value = data[i].Outstanding;
                ws.Cell(row, 8).Value = data[i].Status.ToString();
                ws.Cell(row, 9).Value = data[i].IssuedAt;
                ws.Cell(row, 10).Value = data[i].ExpectedReturnDate;
            }

            ws.Columns().AdjustToContents();
        });
    }

    private static byte[] GenerateExcel(string title, Action<XLWorkbook> configure)
    {
        using var workbook = new XLWorkbook();
        configure(workbook);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void StyleHeader(IXLWorksheet ws, int columnCount)
    {
        var headerRange = ws.Range(1, 1, 1, columnCount);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
    }
}
