using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Books.DTOs;

public class StockMovementResponse
{
    public long Id { get; set; }
    public int BookId { get; set; }
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Notes { get; set; }
    public int ProcessedById { get; set; }
    public DateTime ProcessedAt { get; set; }
}
