namespace BooksPortal.Application.Features.TeacherIssues.DTOs;

public class ProcessTeacherReturnRequest
{
    public List<TeacherReturnItemRequest> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class TeacherReturnItemRequest
{
    public int TeacherIssueItemId { get; set; }
    public int Quantity { get; set; }
}
