namespace BooksPortal.Application.Features.MasterData.DTOs;

public class UpdateAcademicYearRequest
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
