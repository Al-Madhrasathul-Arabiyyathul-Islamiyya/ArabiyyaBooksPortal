using BooksPortal.Domain.Enums;

namespace BooksPortal.Application.Features.Settings.DTOs;

public class CreateReferenceNumberFormatRequest
{
    public SlipType SlipType { get; set; }
    public int AcademicYearId { get; set; }
    public string FormatTemplate { get; set; } = string.Empty;
    public int PaddingWidth { get; set; } = 6;
}
