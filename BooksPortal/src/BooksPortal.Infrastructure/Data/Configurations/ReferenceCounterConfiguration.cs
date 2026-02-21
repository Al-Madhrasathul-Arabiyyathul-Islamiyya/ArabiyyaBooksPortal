using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class ReferenceCounterConfiguration : IEntityTypeConfiguration<ReferenceCounter>
{
    public void Configure(EntityTypeBuilder<ReferenceCounter> builder)
    {
        builder.ToTable("ReferenceCounters");
        builder.HasKey(r => r.Key);
        builder.Property(r => r.Key).HasMaxLength(50);
    }
}
