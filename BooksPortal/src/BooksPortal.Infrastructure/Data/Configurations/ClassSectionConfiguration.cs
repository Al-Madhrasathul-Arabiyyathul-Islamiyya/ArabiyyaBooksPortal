using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class ClassSectionConfiguration : IEntityTypeConfiguration<ClassSection>
{
    public void Configure(EntityTypeBuilder<ClassSection> builder)
    {
        builder.ToTable("ClassSections");

        builder.Property(c => c.Section)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(c => new { c.AcademicYearId, c.GradeId, c.Section })
            .IsUnique();

        builder.HasOne(c => c.AcademicYear)
            .WithMany(a => a.ClassSections)
            .HasForeignKey(c => c.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Keystage)
            .WithMany(k => k.ClassSections)
            .HasForeignKey(c => c.KeystageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Grade)
            .WithMany(g => g.ClassSections)
            .HasForeignKey(c => c.GradeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
