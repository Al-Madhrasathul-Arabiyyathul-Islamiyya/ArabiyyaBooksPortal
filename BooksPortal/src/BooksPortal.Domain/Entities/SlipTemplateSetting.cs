using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class SlipTemplateSetting : BaseEntity
{
    public string Category { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
