using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class ReturnSlipItemConfiguration : IEntityTypeConfiguration<ReturnSlipItem>
{
    public void Configure(EntityTypeBuilder<ReturnSlipItem> builder)
    {
        builder.ToTable("ReturnSlipItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ConditionNotes).HasMaxLength(500);
        builder.HasOne(i => i.Book).WithMany().HasForeignKey(i => i.BookId).OnDelete(DeleteBehavior.Restrict);
    }
}
