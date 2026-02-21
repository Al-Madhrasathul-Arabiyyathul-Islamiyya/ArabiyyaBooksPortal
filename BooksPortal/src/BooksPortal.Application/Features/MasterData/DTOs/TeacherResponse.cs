namespace BooksPortal.Application.Features.MasterData.DTOs;

public class TeacherResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public List<TeacherAssignmentResponse> Assignments { get; set; } = new();
}

public class TeacherAssignmentResponse
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int ClassSectionId { get; set; }
    public string ClassSectionDisplayName { get; set; } = string.Empty;
}
