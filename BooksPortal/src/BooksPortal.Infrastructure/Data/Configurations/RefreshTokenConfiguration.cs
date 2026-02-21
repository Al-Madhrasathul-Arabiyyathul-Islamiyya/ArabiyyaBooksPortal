using BooksPortal.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.CreatedByIp)
            .HasMaxLength(50);

        builder.Property(r => r.RevokedByIp)
            .HasMaxLength(50);

        builder.Property(r => r.ReplacedByToken)
            .HasMaxLength(500);

        builder.HasIndex(r => r.Token)
            .IsUnique();

        builder.HasIndex(r => r.StaffId);

        builder.HasOne(r => r.Staff)
            .WithMany()
            .HasForeignKey(r => r.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
