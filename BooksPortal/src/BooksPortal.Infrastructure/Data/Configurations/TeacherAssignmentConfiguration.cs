using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class TeacherAssignmentConfiguration : IEntityTypeConfiguration<TeacherAssignment>
{
    public void Configure(EntityTypeBuilder<TeacherAssignment> builder)
    {
        builder.ToTable("TeacherAssignments");

        builder.HasIndex(ta => new { ta.TeacherId, ta.SubjectId, ta.ClassSectionId })
            .IsUnique();

        builder.HasOne(ta => ta.Teacher)
            .WithMany(t => t.TeacherAssignments)
            .HasForeignKey(ta => ta.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.Subject)
            .WithMany()
            .HasForeignKey(ta => ta.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.ClassSection)
            .WithMany()
            .HasForeignKey(ta => ta.ClassSectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
