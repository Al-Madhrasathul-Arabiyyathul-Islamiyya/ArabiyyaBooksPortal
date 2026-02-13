using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class DistributionSlipConfiguration : IEntityTypeConfiguration<DistributionSlip>
{
    public void Configure(EntityTypeBuilder<DistributionSlip> builder)
    {
        builder.ToTable("DistributionSlips");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.ReferenceNo).HasMaxLength(50).IsRequired();
        builder.HasIndex(d => d.ReferenceNo).IsUnique();
        builder.Property(d => d.Notes).HasMaxLength(500);
        builder.Property(d => d.LifecycleStatus)
            .IsRequired()
            .HasDefaultValue(BooksPortal.Domain.Enums.SlipLifecycleStatus.Processing);
        builder.HasOne(d => d.AcademicYear).WithMany().HasForeignKey(d => d.AcademicYearId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Student).WithMany().HasForeignKey(d => d.StudentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(d => d.Parent).WithMany().HasForeignKey(d => d.ParentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(d => d.Items).WithOne(i => i.DistributionSlip).HasForeignKey(i => i.DistributionSlipId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(d => d.StudentId);
        builder.HasIndex(d => d.AcademicYearId);
        builder.HasIndex(d => d.LifecycleStatus);
    }
}
