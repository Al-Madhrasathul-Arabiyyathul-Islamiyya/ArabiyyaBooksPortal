using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class AcademicYear : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<ClassSection> ClassSections { get; set; } = new List<ClassSection>();
}
