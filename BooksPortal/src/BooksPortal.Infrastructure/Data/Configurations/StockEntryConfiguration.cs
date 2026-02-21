using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
{
    public void Configure(EntityTypeBuilder<StockEntry> builder)
    {
        builder.ToTable("StockEntries");

        builder.Property(s => s.Source).HasMaxLength(100);
        builder.Property(s => s.Notes).HasMaxLength(500);

        builder.HasOne(s => s.Book)
            .WithMany(b => b.StockEntries)
            .HasForeignKey(s => s.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.AcademicYear)
            .WithMany()
            .HasForeignKey(s => s.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
