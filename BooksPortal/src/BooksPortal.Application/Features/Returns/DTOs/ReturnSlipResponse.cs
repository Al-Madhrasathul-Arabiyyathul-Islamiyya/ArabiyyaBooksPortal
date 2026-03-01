using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Returns.DTOs;

public class ReturnSlipResponse
{
    public int Id { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentIndexNo { get; set; } = string.Empty;
    public string StudentClassName { get; set; } = string.Empty;
    public string? StudentNationalId { get; set; }
    public int ReturnedById { get; set; }
    public string ReturnedByName { get; set; } = string.Empty;
    public string? ReturnedByNationalId { get; set; }
    public string? ReturnedByPhone { get; set; }
    public string? ReturnedByRelationship { get; set; }
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
    public List<ReturnSlipItemResponse> Items { get; set; } = new();
}
