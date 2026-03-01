using BooksPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BooksPortal.Infrastructure.Data.Configurations;

public class TeacherReturnSlipItemConfiguration : IEntityTypeConfiguration<TeacherReturnSlipItem>
{
    public void Configure(EntityTypeBuilder<TeacherReturnSlipItem> builder)
    {
        builder.ToTable("TeacherReturnSlipItems");
        builder.HasKey(i => i.Id);
        builder.HasOne(i => i.TeacherIssueItem).WithMany().HasForeignKey(i => i.TeacherIssueItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Book).WithMany().HasForeignKey(i => i.BookId).OnDelete(DeleteBehavior.Restrict);
    }
}
