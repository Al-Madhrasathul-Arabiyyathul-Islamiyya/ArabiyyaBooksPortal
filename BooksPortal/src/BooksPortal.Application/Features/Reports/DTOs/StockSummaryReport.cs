namespace BooksPortal.Application.Features.Reports.DTOs;

public class StockSummaryReport
{
    public int BookId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string? Grade { get; set; }
    public int TotalStock { get; set; }
    public int Distributed { get; set; }
    public int WithTeachers { get; set; }
    public int Damaged { get; set; }
    public int Lost { get; set; }
    public int Available { get; set; }
}
