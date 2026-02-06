using BooksPortal.Application.Features.Books.DTOs;
using FluentAssertions;

namespace BooksPortal.UnitTests.Features.Books;

public class BookResponseTests
{
    [Fact]
    public void Available_CalculatesCorrectly()
    {
        var response = new BookResponse
        {
            TotalStock = 100,
            Distributed = 30,
            WithTeachers = 10,
            Damaged = 5,
            Lost = 3
        };

        response.Available.Should().Be(52); // 100 - 30 - 10 - 5 - 3
    }

    [Fact]
    public void Available_AllZero_EqualsTotalStock()
    {
        var response = new BookResponse { TotalStock = 50 };

        response.Available.Should().Be(50);
    }

    [Fact]
    public void Available_FullyAllocated_ReturnsZero()
    {
        var response = new BookResponse
        {
            TotalStock = 10,
            Distributed = 5,
            WithTeachers = 3,
            Damaged = 1,
            Lost = 1
        };

        response.Available.Should().Be(0);
    }

    [Fact]
    public void Available_OverAllocated_ReturnsNegative()
    {
        var response = new BookResponse
        {
            TotalStock = 10,
            Distributed = 8,
            WithTeachers = 5
        };

        response.Available.Should().Be(-3);
    }
}
