using BooksPortal.Application.Features.BulkImport.Interfaces;
using ClosedXML.Excel;

namespace BooksPortal.Application.Features.BulkImport.Services;

public class ImportTemplateService : IImportTemplateService
{
    public byte[] CreateBooksTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Books");
        WriteHeaders(ws, "Code", "Title", "SubjectCode", "ISBN", "Author", "Edition", "Publisher", "PublishedYear", "Grade", "AcademicYearId", "Quantity", "Source", "Notes");
        ws.Cell(2, 1).Value = "BK-001";
        ws.Cell(2, 2).Value = "Arabic Grade 1";
        ws.Cell(2, 3).Value = "ARA";
        ws.Cell(2, 4).Value = "9780000000001";
        ws.Cell(2, 5).Value = "Author Name";
        ws.Cell(2, 6).Value = "1st";
        ws.Cell(2, 7).Value = "Other";
        ws.Cell(2, 8).Value = 2026;
        ws.Cell(2, 9).Value = "Grade 1";
        ws.Cell(2, 10).Value = 1;
        ws.Cell(2, 11).Value = 50;
        ws.Cell(2, 12).Value = "MOE";
        ws.Cell(2, 13).Value = "Initial stock";
        return Save(workbook);
    }

    public byte[] CreateTeachersTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Teachers");
        WriteHeaders(ws, "FullName", "NationalId", "Email", "Phone");
        ws.Cell(2, 1).Value = "Teacher Name";
        ws.Cell(2, 2).Value = "A123456";
        ws.Cell(2, 3).Value = "teacher@school.local";
        ws.Cell(2, 4).Value = "7700000";
        return Save(workbook);
    }

    public byte[] CreateStudentsTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Students");
        WriteHeaders(ws, "FullName", "IndexNo", "NationalId", "ClassSectionId");
        ws.Cell(2, 1).Value = "Student Name";
        ws.Cell(2, 2).Value = "IDX-0001";
        ws.Cell(2, 3).Value = "B123456";
        ws.Cell(2, 4).Value = 1;
        return Save(workbook);
    }

    private static void WriteHeaders(IXLWorksheet worksheet, params string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        worksheet.Columns().AdjustToContents();
    }

    private static byte[] Save(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
