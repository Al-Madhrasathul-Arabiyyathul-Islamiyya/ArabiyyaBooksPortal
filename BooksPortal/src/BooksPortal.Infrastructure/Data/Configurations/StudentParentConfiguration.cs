using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class StudentParentConfiguration : IEntityTypeConfiguration<StudentParent>
{
    public void Configure(EntityTypeBuilder<StudentParent> builder)
    {
        builder.ToTable("StudentParents");

        builder.HasKey(sp => new { sp.StudentId, sp.ParentId });

        builder.Property(sp => sp.IsPrimary)
            .HasDefaultValue(false);

        builder.HasOne(sp => sp.Student)
            .WithMany(s => s.StudentParents)
            .HasForeignKey(sp => sp.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.Parent)
            .WithMany(p => p.StudentParents)
            .HasForeignKey(sp => sp.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(sp => !sp.Student.IsDeleted && !sp.Parent.IsDeleted);
    }
}
