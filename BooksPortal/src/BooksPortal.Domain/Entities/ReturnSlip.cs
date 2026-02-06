using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class ReturnSlip : BaseEntity
{
    public string ReferenceNo { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public int StudentId { get; set; }
    public int ReturnedById { get; set; }
    public int ReceivedById { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? Notes { get; set; }
    public string? PdfFilePath { get; set; }

    public AcademicYear AcademicYear { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public List<ReturnSlipItem> Items { get; set; } = new();
}
