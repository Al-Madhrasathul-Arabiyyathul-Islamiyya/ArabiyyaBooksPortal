using System.Text.Json;
using BooksPortal.Application.Common.Interfaces;
using BooksPortal.Domain.Common;
using BooksPortal.Domain.Entities;
using BooksPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BooksPortal.Infrastructure.Data;

public class BooksPortalDbContext : IdentityDbContext<Staff, IdentityRole<int>, int>
{
    private readonly ICurrentUserService _currentUserService;

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Keystage> Keystages => Set<Keystage>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassSection> ClassSections => Set<ClassSection>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<StudentParent> StudentParents => Set<StudentParent>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<TeacherAssignment> TeacherAssignments => Set<TeacherAssignment>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<StockEntry> StockEntries => Set<StockEntry>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<DistributionSlip> DistributionSlips => Set<DistributionSlip>();
    public DbSet<DistributionSlipItem> DistributionSlipItems => Set<DistributionSlipItem>();
    public DbSet<ReferenceCounter> ReferenceCounters => Set<ReferenceCounter>();
    public DbSet<ReturnSlip> ReturnSlips => Set<ReturnSlip>();
    public DbSet<ReturnSlipItem> ReturnSlipItems => Set<ReturnSlipItem>();
    public DbSet<TeacherIssue> TeacherIssues => Set<TeacherIssue>();
    public DbSet<TeacherIssueItem> TeacherIssueItems => Set<TeacherIssueItem>();
    public DbSet<ReferenceNumberFormat> ReferenceNumberFormats => Set<ReferenceNumberFormat>();
    public DbSet<TeacherReturnSlip> TeacherReturnSlips => Set<TeacherReturnSlip>();
    public DbSet<TeacherReturnSlipItem> TeacherReturnSlipItems => Set<TeacherReturnSlipItem>();
    public DbSet<SlipTemplateSetting> SlipTemplateSettings => Set<SlipTemplateSetting>();

    public BooksPortalDbContext(
        DbContextOptions<BooksPortalDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(BooksPortalDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                entityType.AddSoftDeleteQueryFilter();
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries, cancellationToken);
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var userId = _currentUserService.UserId;
        var actor = _currentUserService.IsAuthenticated
            ? (_currentUserService.UserEmail ?? _currentUserService.UserName ?? "system:bulk-import")
            : "system:seed";
        var now = DateTime.UtcNow;
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            if (entry.Entity is IAuditableEntity auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable.CreatedAt = now;
                        auditable.CreatedBy = userId ?? 0;
                        break;
                    case EntityState.Modified:
                        auditable.UpdatedAt = now;
                        auditable.UpdatedBy = userId;
                        break;
                }
            }

            if (entry.Entity is ISoftDeletable softDeletable && softDeletable.IsDeleted && entry.State == EntityState.Modified)
            {
                softDeletable.DeletedAt = now;
            }

            var auditEntry = new AuditEntry
            {
                Action = entry.State switch
                {
                    EntityState.Added => "CREATE",
                    EntityState.Modified => entry.Entity is ISoftDeletable sd && sd.IsDeleted ? "DELETE" : "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => "UNKNOWN"
                },
                EntityType = entry.Entity.GetType().Name,
                UserId = userId,
                UserName = actor
            };

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                var propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.EntityId = property.CurrentValue?.ToString() ?? "";
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified when property.IsModified:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;
                }
            }

            auditEntries.Add(auditEntry);
        }

        foreach (var entry in auditEntries.Where(e => !e.HasTemporaryProperties))
        {
            AuditLogs.Add(entry.ToAuditLog());
        }

        return auditEntries.Where(e => e.HasTemporaryProperties).ToList();
    }

    private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        if (auditEntries.Count == 0)
            return;

        foreach (var entry in auditEntries)
        {
            foreach (var prop in entry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                    entry.EntityId = prop.CurrentValue?.ToString() ?? "";
                else
                    entry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
            }

            AuditLogs.Add(entry.ToAuditLog());
        }

        await base.SaveChangesAsync(cancellationToken);
    }

    private class AuditEntry
    {
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> TemporaryProperties { get; } = new();
        public bool HasTemporaryProperties => TemporaryProperties.Count > 0;

        public AuditLog ToAuditLog() => new()
        {
            Action = Action,
            EntityType = EntityType,
            EntityId = EntityId,
            OldValues = OldValues.Count > 0 ? JsonSerializer.Serialize(OldValues) : null,
            NewValues = NewValues.Count > 0 ? JsonSerializer.Serialize(NewValues) : null,
            UserId = UserId,
            UserName = UserName,
            Timestamp = DateTime.UtcNow
        };
    }
}

public static class SoftDeleteQueryExtension
{
    public static void AddSoftDeleteQueryFilter(this Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
        var falseConstant = System.Linq.Expressions.Expression.Constant(false);
        var condition = System.Linq.Expressions.Expression.Equal(property, falseConstant);
        var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);
        entityType.SetQueryFilter(lambda);
    }
}
