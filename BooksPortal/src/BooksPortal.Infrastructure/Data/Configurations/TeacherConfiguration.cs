using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.Property(t => t.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.NationalId)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(t => t.NationalId)
            .IsUnique();

        builder.Property(t => t.Email)
            .HasMaxLength(256);

        builder.Property(t => t.Phone)
            .HasMaxLength(20);
    }
}
