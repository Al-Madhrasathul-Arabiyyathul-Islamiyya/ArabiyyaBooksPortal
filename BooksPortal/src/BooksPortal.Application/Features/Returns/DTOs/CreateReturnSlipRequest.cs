using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Returns.DTOs;

public class CreateReturnSlipRequest
{
    public int AcademicYearId { get; set; }
    public int StudentId { get; set; }
    public int ReturnedById { get; set; }
    public string? Notes { get; set; }
    public List<CreateReturnSlipItemRequest> Items { get; set; } = new();
}

public class CreateReturnSlipItemRequest
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public BookCondition Condition { get; set; }
    public string? ConditionNotes { get; set; }
}
