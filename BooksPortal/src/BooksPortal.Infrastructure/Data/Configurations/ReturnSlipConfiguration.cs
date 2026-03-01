using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class ReturnSlipConfiguration : IEntityTypeConfiguration<ReturnSlip>
{
    public void Configure(EntityTypeBuilder<ReturnSlip> builder)
    {
        builder.ToTable("ReturnSlips");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ReferenceNo).HasMaxLength(50).IsRequired();
        builder.HasIndex(r => r.ReferenceNo).IsUnique();
        builder.Property(r => r.LifecycleStatus)
            .HasConversion<int>()
            .HasDefaultValue(BooksPortal.Domain.Enums.SlipLifecycleStatus.Processing);
        builder.Property(r => r.Notes).HasMaxLength(500);
        builder.HasOne(r => r.AcademicYear).WithMany().HasForeignKey(r => r.AcademicYearId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Student).WithMany().HasForeignKey(r => r.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(r => r.Items).WithOne(i => i.ReturnSlip).HasForeignKey(i => i.ReturnSlipId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => r.StudentId);
        builder.HasIndex(r => r.AcademicYearId);
        builder.HasIndex(r => r.LifecycleStatus);
    }
}
