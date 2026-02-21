using BooksPortal.Domain.Common;
using BooksPortal.Domain.Enums;

namespace BooksPortal.Domain.Entities;

public class ReferenceNumberFormat : BaseEntity
{
    public SlipType SlipType { get; set; }
    public int AcademicYearId { get; set; }
    public string FormatTemplate { get; set; } = string.Empty;
    public int PaddingWidth { get; set; } = 6;

    public AcademicYear AcademicYear { get; set; } = null!;
}
