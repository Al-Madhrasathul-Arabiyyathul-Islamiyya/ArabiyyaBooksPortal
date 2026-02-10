using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("Grades");

        builder.Property(g => g.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(g => new { g.KeystageId, g.Code })
            .IsUnique();

        builder.HasOne(g => g.Keystage)
            .WithMany(k => k.Grades)
            .HasForeignKey(g => g.KeystageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
