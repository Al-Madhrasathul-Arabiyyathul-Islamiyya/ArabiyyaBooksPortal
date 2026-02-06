using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Reports.DTOs;

public class TeacherOutstandingReport
{
    public int IssueId { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string BookCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public int Outstanding { get; set; }
    public TeacherIssueStatus Status { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
}
