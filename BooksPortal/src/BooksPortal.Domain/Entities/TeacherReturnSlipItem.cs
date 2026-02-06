using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class TeacherReturnSlipItem : BaseEntity
{
    public int TeacherReturnSlipId { get; set; }
    public int TeacherIssueItemId { get; set; }
    public int BookId { get; set; }
    public int Quantity { get; set; }

    public TeacherReturnSlip TeacherReturnSlip { get; set; } = null!;
    public TeacherIssueItem TeacherIssueItem { get; set; } = null!;
    public Book Book { get; set; } = null!;
}
