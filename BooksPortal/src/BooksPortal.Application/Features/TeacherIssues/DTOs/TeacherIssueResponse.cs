using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.TeacherIssues.DTOs;

public class TeacherIssueResponse
{
    public int Id { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string? TeacherNationalId { get; set; }
    public int IssuedById { get; set; }
    public string IssuedByName { get; set; } = string.Empty;
    public string? IssuedByDesignation { get; set; }
    public DateTime IssuedAt { get; set; }
    public SlipLifecycleStatus LifecycleStatus { get; set; }
    public int? FinalizedById { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int? CancelledById { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public TeacherIssueStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? PdfFilePath { get; set; }
    public List<TeacherIssueItemResponse> Items { get; set; } = new();
}
