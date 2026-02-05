using BooksPortal.Domain.Enums;

namespace BooksPortal.Domain.Entities;

public class StockMovement
{
    public long Id { get; set; }
    public int BookId { get; set; }
    public int AcademicYearId { get; set; }
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Notes { get; set; }
    public int ProcessedById { get; set; }
    public DateTime ProcessedAt { get; set; }

    public Book Book { get; set; } = null!;
    public AcademicYear AcademicYear { get; set; } = null!;
}
