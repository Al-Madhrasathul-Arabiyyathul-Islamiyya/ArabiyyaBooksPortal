using BooksPortal.Domain.Common;
using BooksPortal.Domain.Enums;

namespace BooksPortal.Domain.Entities;

public class SystemSetupState : BaseEntity
{
    public SetupStatus Status { get; set; } = SetupStatus.NotStarted;
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? SuperAdminConfirmedAtUtc { get; set; }
    public DateTime? SlipTemplatesConfirmedAtUtc { get; set; }
    public DateTime? HierarchyInitializedAtUtc { get; set; }
    public DateTime? ActiveAcademicYearValidatedAtUtc { get; set; }
    public DateTime? ReferenceFormatsInitializedAtUtc { get; set; }
    public DateTime? LastEvaluatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string SchemaVersion { get; set; } = "1";
}
