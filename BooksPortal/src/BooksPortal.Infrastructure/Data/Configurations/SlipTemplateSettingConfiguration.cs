using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class SlipTemplateSettingConfiguration : IEntityTypeConfiguration<SlipTemplateSetting>
{
    public void Configure(EntityTypeBuilder<SlipTemplateSetting> builder)
    {
        builder.ToTable("SlipTemplateSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Category).HasMaxLength(50).IsRequired();
        builder.Property(s => s.Key).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Value).HasMaxLength(500).IsRequired();
        builder.HasIndex(s => new { s.Category, s.Key }).IsUnique();
        builder.HasIndex(s => s.Category);
    }
}
