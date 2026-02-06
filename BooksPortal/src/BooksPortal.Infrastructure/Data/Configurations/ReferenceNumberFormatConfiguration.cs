using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class ReferenceNumberFormatConfiguration : IEntityTypeConfiguration<ReferenceNumberFormat>
{
    public void Configure(EntityTypeBuilder<ReferenceNumberFormat> builder)
    {
        builder.ToTable("ReferenceNumberFormats");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.FormatTemplate).HasMaxLength(100).IsRequired();
        builder.Property(r => r.PaddingWidth).HasDefaultValue(6);
        builder.HasIndex(r => new { r.SlipType, r.AcademicYearId }).IsUnique();
        builder.HasOne(r => r.AcademicYear).WithMany().HasForeignKey(r => r.AcademicYearId).OnDelete(DeleteBehavior.Restrict);
    }
}
