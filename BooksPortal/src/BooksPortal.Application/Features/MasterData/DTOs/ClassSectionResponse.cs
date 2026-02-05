namespace BooksPortal.Application.Features.MasterData.DTOs;

public class ClassSectionResponse
{
    public int Id { get; set; }
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public int KeystageId { get; set; }
    public string KeystageName { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string DisplayName => $"{Grade} {Section}";
    public int StudentCount { get; set; }
}
