namespace BooksPortal.Application.Features.Books.DTOs;

public class AddStockRequest
{
    public int AcademicYearId { get; set; }
    public int Quantity { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
}
