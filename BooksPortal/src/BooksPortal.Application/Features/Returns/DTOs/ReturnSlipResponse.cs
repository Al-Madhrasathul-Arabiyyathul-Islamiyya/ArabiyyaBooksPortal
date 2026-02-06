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
    public int ReturnedById { get; set; }
    public string ReturnedByName { get; set; } = string.Empty;
    public int ReceivedById { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? Notes { get; set; }
    public List<ReturnSlipItemResponse> Items { get; set; } = new();
}
