namespace BooksPortal.Application.Features.Reports.DTOs;

public class DistributionSummaryReport
{
    public int SlipId { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentIndexNo { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public int TotalBooks { get; set; }
}
