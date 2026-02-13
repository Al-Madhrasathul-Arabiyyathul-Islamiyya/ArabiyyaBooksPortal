namespace BooksPortal.Application.Features.MasterData.DTOs;

public class StudentOperationsLookupResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string IndexNo { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public int ClassSectionId { get; set; }
    public string ClassSectionName { get; set; } = string.Empty;
    public ParentLookupSummaryResponse? PrimaryParent { get; set; }
}

public class ParentLookupSummaryResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Phone { get; set; }
    public string? Relationship { get; set; }
}

public class BookOperationsLookupResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int AvailableStock { get; set; }
}

public class TeacherOperationsLookupResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public List<string> Assignments { get; set; } = new();
}

public class TeacherIssueOutstandingLookupResponse
{
    public int IssueId { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public List<TeacherIssueOutstandingItemResponse> Items { get; set; } = new();
}

public class TeacherIssueOutstandingItemResponse
{
    public int TeacherIssueItemId { get; set; }
    public int BookId { get; set; }
    public string BookCode { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public int OutstandingQuantity { get; set; }
}
