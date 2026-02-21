using BooksPortal.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.Property(s => s.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.NationalId)
            .HasMaxLength(50);

        builder.Property(s => s.Designation)
            .HasMaxLength(100);

        builder.HasIndex(s => s.NationalId)
            .IsUnique()
            .HasFilter("[NationalId] IS NOT NULL");
    }
}
