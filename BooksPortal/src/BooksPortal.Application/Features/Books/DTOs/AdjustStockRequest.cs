using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Books.DTOs;

public class AdjustStockRequest
{
    public int AcademicYearId { get; set; }
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}
