using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Distribution.DTOs;

public class UpdateDistributionSlipRequest
{
    public Term Term { get; set; }
    public int StudentId { get; set; }
    public int ParentId { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public TimeOnly? IssuedTime { get; set; }
    public string? Notes { get; set; }
    public List<UpdateDistributionSlipItemRequest> Items { get; set; } = new();
}

public class UpdateDistributionSlipItemRequest
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
}
