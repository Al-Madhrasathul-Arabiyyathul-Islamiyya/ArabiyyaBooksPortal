using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class Student : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string IndexNo { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public int ClassSectionId { get; set; }

    public ClassSection ClassSection { get; set; } = null!;
    public ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
}
