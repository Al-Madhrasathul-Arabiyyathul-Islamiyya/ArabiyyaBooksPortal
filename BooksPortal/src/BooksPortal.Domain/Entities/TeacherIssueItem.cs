using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class TeacherIssueItem : BaseEntity
{
    public int TeacherIssueId { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int? ReceivedById { get; set; }

    public int OutstandingQuantity => Quantity - ReturnedQuantity;

    public TeacherIssue TeacherIssue { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
