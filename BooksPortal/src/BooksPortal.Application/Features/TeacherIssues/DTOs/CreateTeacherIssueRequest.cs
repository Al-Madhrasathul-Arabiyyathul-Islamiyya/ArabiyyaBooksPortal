namespace BooksPortal.Application.Features.TeacherIssues.DTOs;

public class CreateTeacherIssueRequest
{
    public int AcademicYearId { get; set; }
    public int TeacherId { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateTeacherIssueItemRequest> Items { get; set; } = new();
}

public class CreateTeacherIssueItemRequest
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
}
