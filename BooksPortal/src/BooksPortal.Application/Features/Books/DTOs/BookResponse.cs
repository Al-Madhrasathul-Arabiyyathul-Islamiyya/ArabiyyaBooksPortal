namespace BooksPortal.Application.Features.Books.DTOs;

public class BookResponse
{
    public int Id { get; set; }
    public string? ISBN { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Edition { get; set; }
    public string? Publisher { get; set; }
    public int? PublishedYear { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? Grade { get; set; }
    public int TotalStock { get; set; }
    public int Distributed { get; set; }
    public int WithTeachers { get; set; }
    public int Damaged { get; set; }
    public int Lost { get; set; }
    public int Available => TotalStock - Distributed - WithTeachers - Damaged - Lost;
}
