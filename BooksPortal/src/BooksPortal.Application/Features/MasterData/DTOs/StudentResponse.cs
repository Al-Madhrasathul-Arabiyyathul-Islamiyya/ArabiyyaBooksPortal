namespace BooksPortal.Application.Features.MasterData.DTOs;

public class StudentResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IndexNo { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public int ClassSectionId { get; set; }
    public string ClassSectionDisplayName { get; set; } = string.Empty;
    public List<StudentParentResponse> Parents { get; set; } = new();
}

public class StudentParentResponse
{
    public int ParentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Relationship { get; set; }
    public bool IsPrimary { get; set; }
}
