using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Returns.DTOs;

public class ReturnSlipItemResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public BookCondition Condition { get; set; }
    public string? ConditionNotes { get; set; }
}
