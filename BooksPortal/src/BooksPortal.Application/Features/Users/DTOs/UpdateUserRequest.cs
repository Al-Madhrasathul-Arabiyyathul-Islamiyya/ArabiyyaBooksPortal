namespace BooksPortal.Application.Features.Users.DTOs;

public class UpdateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Designation { get; set; }
    public bool IsActive { get; set; }
}
