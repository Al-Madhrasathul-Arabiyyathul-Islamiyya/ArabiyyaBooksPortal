using BooksPortal.Domain.Common;
using FluentAssertions;

namespace BooksPortal.UnitTests;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity { }

    [Fact]
    public void NewEntity_ShouldHaveDefaultValues()
    {
        var entity = new TestEntity();

        entity.Id.Should().Be(0);
        entity.IsDeleted.Should().BeFalse();
        entity.CreatedAt.Should().Be(default);
        entity.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedTrue()
    {
        var entity = new TestEntity();
        entity.IsDeleted = true;

        entity.IsDeleted.Should().BeTrue();
    }
}
