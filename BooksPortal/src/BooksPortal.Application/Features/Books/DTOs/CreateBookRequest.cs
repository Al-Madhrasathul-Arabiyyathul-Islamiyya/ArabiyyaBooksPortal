namespace BooksPortal.Application.Features.Books.DTOs;

public class CreateBookRequest
{
    public string? ISBN { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Edition { get; set; }
    public string Publisher { get; set; } = "Other";
    public int PublishedYear { get; set; }
    public int SubjectId { get; set; }
    public string? Grade { get; set; }
}
