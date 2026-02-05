using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.Property(s => s.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.IndexNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.IndexNo)
            .IsUnique();

        builder.Property(s => s.NationalId)
            .HasMaxLength(50);

        builder.HasOne(s => s.ClassSection)
            .WithMany(c => c.Students)
            .HasForeignKey(s => s.ClassSectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
