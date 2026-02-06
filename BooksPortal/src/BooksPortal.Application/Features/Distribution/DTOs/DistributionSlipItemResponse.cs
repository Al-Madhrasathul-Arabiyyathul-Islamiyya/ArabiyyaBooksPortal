namespace BooksPortal.Application.Features.Distribution.DTOs;

public class DistributionSlipItemResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
