using BooksPortal.Domain.Common;

namespace BooksPortal.Domain.Entities;

public class Subject : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
