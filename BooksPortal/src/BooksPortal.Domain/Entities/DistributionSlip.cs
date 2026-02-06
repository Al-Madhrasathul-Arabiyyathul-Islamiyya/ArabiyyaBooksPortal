using BooksPortal.Domain.Common;
using BooksPortal.Domain.Enums;

namespace BooksPortal.Domain.Entities;

public class DistributionSlip : BaseEntity
{
    public string ReferenceNo { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public Term Term { get; set; }
    public int StudentId { get; set; }
    public int ParentId { get; set; }
    public int IssuedById { get; set; }
    public DateTime IssuedAt { get; set; }
    public string? Notes { get; set; }
    public string? PdfFilePath { get; set; }

    public AcademicYear AcademicYear { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public Parent Parent { get; set; } = null!;
    public List<DistributionSlipItem> Items { get; set; } = new();
}
