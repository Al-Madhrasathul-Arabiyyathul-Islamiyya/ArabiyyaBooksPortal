using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Distribution.DTOs;

public class DistributionSlipResponse
{
    public int Id { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public Term Term { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentIndexNo { get; set; } = string.Empty;
    public string StudentClassName { get; set; } = string.Empty;
    public string? StudentNationalId { get; set; }
    public int ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string? ParentNationalId { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentRelationship { get; set; }
    public int IssuedById { get; set; }
    public DateTime IssuedAt { get; set; }
    public SlipLifecycleStatus LifecycleStatus { get; set; }
    public int? FinalizedById { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int? CancelledById { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? Notes { get; set; }
    public string? PdfFilePath { get; set; }
    public List<DistributionSlipItemResponse> Items { get; set; } = new();
}
