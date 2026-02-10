using BooksPortal.Application.Features.Books.DTOs;
using BooksPortal.Application.Features.Books.Validators;
using FluentAssertions;

namespace BooksPortal.UnitTests.Features.Books;

public class CreateBookRequestValidatorTests
{
    private readonly CreateBookRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenPublisherMissing_ShouldFail()
    {
        var request = new CreateBookRequest
        {
            Code = "BK-001",
            Title = "Test Book",
            SubjectId = 1,
            Publisher = "",
            PublishedYear = 2026
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateBookRequest.Publisher));
    }

    [Fact]
    public void Validate_WhenPublishedYearOutOfRange_ShouldFail()
    {
        var request = new CreateBookRequest
        {
            Code = "BK-001",
            Title = "Test Book",
            SubjectId = 1,
            Publisher = "Other",
            PublishedYear = 1800
        };

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateBookRequest.PublishedYear));
    }
}
