using BooksPortal.Domain.Common;
using BooksPortal.Domain.Enums;

namespace BooksPortal.Domain.Entities;

public class TeacherReturnSlip : BaseEntity
{
    public string ReferenceNo { get; set; } = string.Empty;
    public int TeacherIssueId { get; set; }
    public int ReceivedById { get; set; }
    public DateTime ReceivedAt { get; set; }
    public SlipLifecycleStatus LifecycleStatus { get; set; } = SlipLifecycleStatus.Processing;
    public int? FinalizedById { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int? CancelledById { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? Notes { get; set; }
    public string? PdfFilePath { get; set; }

    public TeacherIssue TeacherIssue { get; set; } = null!;
    public List<TeacherReturnSlipItem> Items { get; set; } = new();
}
