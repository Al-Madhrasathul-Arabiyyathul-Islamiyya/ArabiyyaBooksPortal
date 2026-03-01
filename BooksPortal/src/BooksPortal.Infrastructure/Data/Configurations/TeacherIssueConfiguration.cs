using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class TeacherIssueConfiguration : IEntityTypeConfiguration<TeacherIssue>
{
    public void Configure(EntityTypeBuilder<TeacherIssue> builder)
    {
        builder.ToTable("TeacherIssues");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.ReferenceNo).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.ReferenceNo).IsUnique();
        builder.Property(t => t.Notes).HasMaxLength(500);
        builder.Property(t => t.LifecycleStatus)
            .IsRequired()
            .HasDefaultValue(BooksPortal.Domain.Enums.SlipLifecycleStatus.Processing);
        builder.HasOne(t => t.AcademicYear).WithMany().HasForeignKey(t => t.AcademicYearId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Teacher).WithMany().HasForeignKey(t => t.TeacherId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(t => t.Items).WithOne(i => i.TeacherIssue).HasForeignKey(i => i.TeacherIssueId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(t => t.TeacherId);
        builder.HasIndex(t => t.AcademicYearId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.LifecycleStatus);
    }
}
