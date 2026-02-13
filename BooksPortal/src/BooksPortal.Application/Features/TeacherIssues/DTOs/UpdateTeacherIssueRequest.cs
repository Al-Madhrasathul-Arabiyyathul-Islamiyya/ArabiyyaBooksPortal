namespace BooksPortal.Application.Features.TeacherIssues.DTOs;

public class UpdateTeacherIssueRequest
{
    public int TeacherId { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public TimeOnly? IssuedTime { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public string? Notes { get; set; }
    public List<UpdateTeacherIssueItemRequest> Items { get; set; } = new();
}

public class UpdateTeacherIssueItemRequest
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
}
