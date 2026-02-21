using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class StockEntry : BaseEntity
{
    public int BookId { get; set; }
    public int AcademicYearId { get; set; }
    public int Quantity { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public int EnteredById { get; set; }
    public DateTime EnteredAt { get; set; }

    public Book Book { get; set; } = null!;
    public AcademicYear AcademicYear { get; set; } = null!;
}
