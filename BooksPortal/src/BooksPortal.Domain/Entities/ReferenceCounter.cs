namespace BooksPortal.Domain.Entities;

public class ReferenceCounter
{
    public string Key { get; set; } = string.Empty;
    public int LastSequence { get; set; }
}
