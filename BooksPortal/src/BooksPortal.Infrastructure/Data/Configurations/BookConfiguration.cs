using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.Property(b => b.ISBN).HasMaxLength(20);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(b => b.Code).IsUnique();
        builder.Property(b => b.Title).IsRequired().HasMaxLength(500);
        builder.Property(b => b.Author).HasMaxLength(300);
        builder.Property(b => b.Edition).HasMaxLength(50);
        builder.Property(b => b.Publisher).IsRequired().HasMaxLength(200);
        builder.Property(b => b.PublishedYear).IsRequired();
        builder.Property(b => b.Grade).HasMaxLength(20);

        builder.Property(b => b.TotalStock).HasDefaultValue(0);
        builder.Property(b => b.Distributed).HasDefaultValue(0);
        builder.Property(b => b.WithTeachers).HasDefaultValue(0);
        builder.Property(b => b.Damaged).HasDefaultValue(0);
        builder.Property(b => b.Lost).HasDefaultValue(0);

        builder.HasOne(b => b.Subject)
            .WithMany()
            .HasForeignKey(b => b.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
