using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class SystemSetupStateConfiguration : IEntityTypeConfiguration<SystemSetupState>
{
    public void Configure(EntityTypeBuilder<SystemSetupState> builder)
    {
        builder.ToTable("SystemSetupState");

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.SchemaVersion)
            .HasMaxLength(16)
            .IsRequired();

        builder.HasData(new SystemSetupState
        {
            Id = 1,
            Status = Domain.Enums.SetupStatus.NotStarted,
            SchemaVersion = "1",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedBy = 0,
            IsDeleted = false
        });
    }
}
