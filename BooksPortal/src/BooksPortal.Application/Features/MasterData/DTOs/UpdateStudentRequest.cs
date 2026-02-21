namespace BooksPortal.Application.Features.MasterData.DTOs;

public class UpdateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public int ClassSectionId { get; set; }
    public List<StudentParentRequest>? Parents { get; set; }
}
