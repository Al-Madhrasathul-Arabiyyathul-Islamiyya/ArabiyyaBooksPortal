using BooksPortal.Domain.Common;
using BooksPortal.Domain.Enums;

namespace BooksPortal.Domain.Entities;

public class TeacherIssue : BaseEntity
{
    public string ReferenceNo { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public int TeacherId { get; set; }
    public int IssuedById { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public TeacherIssueStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? PdfFilePath { get; set; }

    public AcademicYear AcademicYear { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public List<TeacherIssueItem> Items { get; set; } = new();
}
