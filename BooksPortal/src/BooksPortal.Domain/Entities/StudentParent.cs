namespace BooksPortal.Domain.Entities;

public class StudentParent
{
    public int StudentId { get; set; }
    public int ParentId { get; set; }
    public bool IsPrimary { get; set; }

    public Student Student { get; set; } = null!;
    public Parent Parent { get; set; } = null!;
}
