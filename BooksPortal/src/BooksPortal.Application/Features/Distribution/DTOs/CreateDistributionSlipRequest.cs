using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Distribution.DTOs;

public class CreateDistributionSlipRequest
{
    public int AcademicYearId { get; set; }
    public Term Term { get; set; }
    public int StudentId { get; set; }
    public int ParentId { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public TimeOnly? IssuedTime { get; set; }
    public string? Notes { get; set; }
    public List<CreateDistributionSlipItemRequest> Items { get; set; } = new();
}

public class CreateDistributionSlipItemRequest
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
}
