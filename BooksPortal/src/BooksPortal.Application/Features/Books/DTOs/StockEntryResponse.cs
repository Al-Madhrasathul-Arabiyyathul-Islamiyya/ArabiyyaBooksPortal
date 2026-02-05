namespace BooksPortal.Application.Features.Books.DTOs;

public class StockEntryResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int AcademicYearId { get; set; }
    public int Quantity { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public int EnteredById { get; set; }
    public DateTime EnteredAt { get; set; }
}
