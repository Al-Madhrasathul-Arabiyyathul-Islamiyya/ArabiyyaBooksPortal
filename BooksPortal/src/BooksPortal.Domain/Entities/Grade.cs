using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class Grade : BaseEntity
{
    public int KeystageId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Keystage Keystage { get; set; } = null!;
    public ICollection<ClassSection> ClassSections { get; set; } = new List<ClassSection>();
}
