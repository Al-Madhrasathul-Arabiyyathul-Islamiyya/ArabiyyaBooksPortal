namespace BooksPortal.Application.Features.Auth.DTOs;

public class UserProfileResponse
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Designation { get; set; }
    public List<string> Roles { get; set; } = [];
}
