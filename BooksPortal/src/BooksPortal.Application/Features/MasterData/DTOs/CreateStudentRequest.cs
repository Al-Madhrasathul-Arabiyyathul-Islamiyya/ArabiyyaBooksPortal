namespace BooksPortal.Application.Features.MasterData.DTOs;

public class CreateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string IndexNo { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public int ClassSectionId { get; set; }
    public List<StudentParentRequest>? Parents { get; set; }
}

public class StudentParentRequest
{
    public int ParentId { get; set; }
    public bool IsPrimary { get; set; }
}
