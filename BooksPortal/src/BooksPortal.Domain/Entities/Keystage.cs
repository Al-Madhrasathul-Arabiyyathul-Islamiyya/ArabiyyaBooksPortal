using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class Keystage : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<ClassSection> ClassSections { get; set; } = new List<ClassSection>();
}
