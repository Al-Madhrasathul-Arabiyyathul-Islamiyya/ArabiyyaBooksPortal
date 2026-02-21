namespace BooksPortal.Application.Features.Reports.DTOs;

public class StudentHistoryReport
{
    public string Type { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Condition { get; set; }
}
