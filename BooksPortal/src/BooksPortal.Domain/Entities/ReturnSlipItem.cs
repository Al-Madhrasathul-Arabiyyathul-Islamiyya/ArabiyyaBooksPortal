using BooksPortal.Domain.Common;
using BooksPortal.Domain.Enums;

namespace BooksPortal.Domain.Entities;

public class ReturnSlipItem : BaseEntity
{
    public int ReturnSlipId { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public BookCondition Condition { get; set; }
    public string? ConditionNotes { get; set; }

    public ReturnSlip ReturnSlip { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
