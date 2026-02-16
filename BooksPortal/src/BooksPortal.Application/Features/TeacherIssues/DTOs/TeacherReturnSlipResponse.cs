using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.TeacherIssues.DTOs;

public class TeacherReturnSlipResponse
{
    public int Id { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public int TeacherIssueId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string? TeacherNationalId { get; set; }
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public int ReceivedById { get; set; }
    public string ReceivedByName { get; set; } = string.Empty;
    public string? ReceivedByDesignation { get; set; }
    public DateTime ReceivedAt { get; set; }
    public SlipLifecycleStatus LifecycleStatus { get; set; }
    public int? FinalizedById { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int? CancelledById { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? Notes { get; set; }
    public string? PdfFilePath { get; set; }
    public List<TeacherReturnSlipItemResponse> Items { get; set; } = new();
}

public class TeacherReturnSlipItemResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
