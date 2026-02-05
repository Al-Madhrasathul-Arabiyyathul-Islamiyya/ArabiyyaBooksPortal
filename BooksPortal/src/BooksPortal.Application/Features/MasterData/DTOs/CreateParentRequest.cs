namespace BooksPortal.Application.Features.MasterData.DTOs;

public class CreateParentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Relationship { get; set; }
}
