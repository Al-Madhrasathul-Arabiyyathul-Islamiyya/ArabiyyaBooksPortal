using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class DistributionSlipItemConfiguration : IEntityTypeConfiguration<DistributionSlipItem>
{
    public void Configure(EntityTypeBuilder<DistributionSlipItem> builder)
    {
        builder.ToTable("DistributionSlipItems");
        builder.HasKey(i => i.Id);
        builder.HasOne(i => i.Book).WithMany().HasForeignKey(i => i.BookId).OnDelete(DeleteBehavior.Restrict);
    }
}
