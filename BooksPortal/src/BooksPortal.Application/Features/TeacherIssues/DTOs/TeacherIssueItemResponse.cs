namespace BooksPortal.Application.Features.TeacherIssues.DTOs;

public class TeacherIssueItemResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public int OutstandingQuantity { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int? ReceivedById { get; set; }
}
