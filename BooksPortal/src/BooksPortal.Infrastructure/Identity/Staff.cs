using BooksPortal.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace BooksPortal.Infrastructure.Identity;

public class Staff : IdentityUser<int>, IAuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Designation { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
}
