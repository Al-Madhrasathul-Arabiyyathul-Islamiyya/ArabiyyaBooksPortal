using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.Property(s => s.ReferenceType).HasMaxLength(50);
        builder.Property(s => s.Notes).HasMaxLength(500);

        builder.HasOne(s => s.Book)
            .WithMany(b => b.StockMovements)
            .HasForeignKey(s => s.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.AcademicYear)
            .WithMany()
            .HasForeignKey(s => s.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(s => !s.AcademicYear.IsDeleted && !s.Book.IsDeleted);

        builder.HasIndex(s => s.BookId);
        builder.HasIndex(s => s.ProcessedAt);
    }
}
