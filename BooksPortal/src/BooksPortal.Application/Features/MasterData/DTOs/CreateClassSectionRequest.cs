namespace BooksPortal.Application.Features.MasterData.DTOs;

public class CreateClassSectionRequest
{
    public int AcademicYearId { get; set; }
    public int KeystageId { get; set; }
    public int GradeId { get; set; }
    public string Section { get; set; } = string.Empty;
}
