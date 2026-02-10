using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class Book : BaseEntity
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
    public int TotalStock { get; set; }
    public int Distributed { get; set; }
    public int WithTeachers { get; set; }
    public int Damaged { get; set; }
    public int Lost { get; set; }

    public Subject Subject { get; set; } = null!;
    public ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
