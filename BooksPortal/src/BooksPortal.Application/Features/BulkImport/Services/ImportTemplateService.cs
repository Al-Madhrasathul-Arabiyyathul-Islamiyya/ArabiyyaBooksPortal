using BooksPortal.Application.Features.BulkImport.Interfaces;
using ClosedXML.Excel;

namespace BooksPortal.Application.Features.BulkImport.Services;

public class ImportTemplateService : IImportTemplateService
{
    public byte[] CreateBooksTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Books");
        WriteHeaders(ws, "Code", "Title", "SubjectCode", "ISBN", "Author", "Edition", "Publisher", "PublishedYear", "Grade", "AcademicYear", "Quantity", "Source", "Notes");
        return Save(workbook);
    }

    public byte[] CreateTeachersTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Teachers");
        WriteHeaders(ws, "FullName", "NationalId", "Email", "Phone");
        return Save(workbook);
    }

    public byte[] CreateStudentsTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Students");
        WriteHeaders(ws, "FullName", "IndexNo", "NationalId", "ClassSectionId", "ParentNationalId");
        return Save(workbook);
    }

    public byte[] CreateParentsTemplate()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Parents");
        WriteHeaders(ws, "FullName", "NationalId", "Phone", "Relationship", "StudentIndexNo");
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
