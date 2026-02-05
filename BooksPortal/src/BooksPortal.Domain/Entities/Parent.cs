using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class Parent : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Relationship { get; set; }

    public ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
}
