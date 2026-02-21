using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("AcademicYears");

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(a => a.Year)
            .IsUnique();

        builder.Property(a => a.IsActive)
            .HasDefaultValue(false);
    }
}
