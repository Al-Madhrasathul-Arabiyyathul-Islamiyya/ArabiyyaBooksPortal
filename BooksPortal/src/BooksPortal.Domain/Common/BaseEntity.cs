namespace BooksPortal.Domain.Common;

public abstract class BaseEntity : IAuditableEntity, ISoftDeletable
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
