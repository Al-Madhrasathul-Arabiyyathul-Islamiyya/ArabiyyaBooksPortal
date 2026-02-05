namespace BooksPortal.Application.Features.Users.DTOs;

public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Designation { get; set; }
    public List<string> Roles { get; set; } = [];
}
