using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(s => s.Code)
            .IsUnique();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
