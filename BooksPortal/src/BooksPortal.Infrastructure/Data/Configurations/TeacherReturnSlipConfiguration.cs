using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class TeacherReturnSlipConfiguration : IEntityTypeConfiguration<TeacherReturnSlip>
{
    public void Configure(EntityTypeBuilder<TeacherReturnSlip> builder)
    {
        builder.ToTable("TeacherReturnSlips");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.ReferenceNo).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.ReferenceNo).IsUnique();
        builder.Property(t => t.LifecycleStatus)
            .HasConversion<int>()
            .HasDefaultValue(BooksPortal.Domain.Enums.SlipLifecycleStatus.Processing);
        builder.Property(t => t.Notes).HasMaxLength(500);
        builder.Property(t => t.PdfFilePath).HasMaxLength(500);
        builder.HasOne(t => t.TeacherIssue).WithMany().HasForeignKey(t => t.TeacherIssueId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(t => t.Items).WithOne(i => i.TeacherReturnSlip).HasForeignKey(i => i.TeacherReturnSlipId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(t => t.TeacherIssueId);
        builder.HasIndex(t => t.LifecycleStatus);
    }
}
