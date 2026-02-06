using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class TeacherIssueItemConfiguration : IEntityTypeConfiguration<TeacherIssueItem>
{
    public void Configure(EntityTypeBuilder<TeacherIssueItem> builder)
    {
        builder.ToTable("TeacherIssueItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ReturnedQuantity).HasDefaultValue(0);
        builder.Ignore(i => i.OutstandingQuantity);
        builder.HasOne(i => i.Book).WithMany().HasForeignKey(i => i.BookId).OnDelete(DeleteBehavior.Restrict);
    }
}
