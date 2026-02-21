using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class KeystageConfiguration : IEntityTypeConfiguration<Keystage>
{
    public void Configure(EntityTypeBuilder<Keystage> builder)
    {
        builder.ToTable("Keystages");

        builder.Property(k => k.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(k => k.Code)
            .IsUnique();

        builder.Property(k => k.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
