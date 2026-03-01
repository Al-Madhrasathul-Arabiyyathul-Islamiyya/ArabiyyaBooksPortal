namespace BooksPortal.Infrastructure.Data;

public class SeedAccountSettings
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string Designation { get; set; } = string.Empty;
}
