using ClosedXML.Excel;
using BooksPortal.Application.Common.Exceptions;

namespace BooksPortal.Application.Features.BulkImport.Services;

internal static class BulkImportWorksheetReader
{
    public static XLWorkbook OpenWorkbook(Stream stream)
    {
        stream.Position = 0;
        try
        {
            return new XLWorkbook(stream);
        }
        catch (Exception)
        {
            throw new BadRequestException("Uploaded file is not a valid Excel workbook (.xlsx).");
        }
    }

    public static Dictionary<string, int> ReadHeaderMap(IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed()
            ?? throw new InvalidOperationException("Workbook is empty.");

        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in usedRange.FirstRow().CellsUsed())
        {
            var value = cell.GetString().Trim();
            if (!string.IsNullOrWhiteSpace(value) && !headers.ContainsKey(value))
                headers[value] = cell.Address.ColumnNumber;
        }

        return headers;
    }

    public static string Cell(IXLWorksheet worksheet, int row, Dictionary<string, int> headers, string headerName)
    {
        if (!headers.TryGetValue(headerName, out var col))
            return string.Empty;
        return worksheet.Cell(row, col).GetString().Trim();
    }

    public static int LastDataRow(IXLWorksheet worksheet)
    {
        var usedRange = worksheet.RangeUsed();
        if (usedRange is null)
            return 1;
        return usedRange.LastRow().RowNumber();
    }
}
