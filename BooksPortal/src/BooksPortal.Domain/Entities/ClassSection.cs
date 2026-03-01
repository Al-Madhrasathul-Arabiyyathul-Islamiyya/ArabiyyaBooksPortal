using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class ClassSection : BaseEntity
{
    public int AcademicYearId { get; set; }
    public int KeystageId { get; set; }
    public int GradeId { get; set; }
    public string Section { get; set; } = string.Empty;

    public AcademicYear AcademicYear { get; set; } = null!;
    public Keystage Keystage { get; set; } = null!;
    public Grade Grade { get; set; } = null!;
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
