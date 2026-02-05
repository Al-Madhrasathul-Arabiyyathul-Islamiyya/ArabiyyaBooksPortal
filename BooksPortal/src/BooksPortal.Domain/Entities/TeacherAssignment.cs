using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class TeacherAssignment : BaseEntity
{
    public int TeacherId { get; set; }
    public int SubjectId { get; set; }
    public int ClassSectionId { get; set; }

    public Teacher Teacher { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ClassSection ClassSection { get; set; } = null!;
}
