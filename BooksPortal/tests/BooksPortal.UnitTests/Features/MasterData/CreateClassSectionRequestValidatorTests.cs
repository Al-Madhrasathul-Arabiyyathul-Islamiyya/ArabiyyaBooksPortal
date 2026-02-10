using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Validators;
using FluentAssertions;

namespace BooksPortal.UnitTests.Features.MasterData;

public class CreateClassSectionRequestValidatorTests
{
    [Fact]
    public void Validate_WhenGradeIdMissing_ShouldFail()
    {
        var validator = new CreateClassSectionRequestValidator();
        var request = new CreateClassSectionRequest
        {
            AcademicYearId = 1,
            KeystageId = 1,
            GradeId = 0,
            Section = "A"
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateClassSectionRequest.GradeId));
    }
}
