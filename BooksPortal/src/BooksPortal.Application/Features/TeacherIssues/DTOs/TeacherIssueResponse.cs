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
    public int IssuedById { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public TeacherIssueStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? PdfFilePath { get; set; }
    public List<TeacherIssueItemResponse> Items { get; set; } = new();
}
