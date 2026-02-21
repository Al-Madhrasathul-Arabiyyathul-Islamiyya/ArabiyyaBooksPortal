using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class ParentConfiguration : IEntityTypeConfiguration<Parent>
{
    public void Configure(EntityTypeBuilder<Parent> builder)
    {
        builder.ToTable("Parents");

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.NationalId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.NationalId)
            .IsUnique();

        builder.Property(p => p.Phone)
            .HasMaxLength(20);

        builder.Property(p => p.Relationship)
            .HasMaxLength(50);
    }
}
