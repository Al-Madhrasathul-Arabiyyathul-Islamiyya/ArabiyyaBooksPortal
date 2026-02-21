using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Settings.DTOs;

public class ReferenceNumberFormatResponse
{
    public int Id { get; set; }
    public SlipType SlipType { get; set; }
    public int AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public string FormatTemplate { get; set; } = string.Empty;
    public int PaddingWidth { get; set; }
}
