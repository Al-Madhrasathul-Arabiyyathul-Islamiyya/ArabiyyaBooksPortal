using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class Teacher : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
}
