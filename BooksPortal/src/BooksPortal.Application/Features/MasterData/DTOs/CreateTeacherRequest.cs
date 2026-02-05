namespace BooksPortal.Application.Features.MasterData.DTOs;

public class CreateTeacherRequest
{
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
