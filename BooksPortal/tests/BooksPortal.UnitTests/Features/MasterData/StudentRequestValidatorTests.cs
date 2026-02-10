using BooksPortal.Application.Features.MasterData.DTOs;
using BooksPortal.Application.Features.MasterData.Validators;
using FluentAssertions;

namespace BooksPortal.UnitTests.Features.MasterData;

public class StudentRequestValidatorTests
{
    [Fact]
    public void CreateValidator_WhenNationalIdMissing_ShouldFail()
    {
        var validator = new CreateStudentRequestValidator();
        var request = new CreateStudentRequest
        {
            FullName = "Test Student",
            IndexNo = "IDX-1",
            NationalId = "",
            ClassSectionId = 1
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CreateStudentRequest.NationalId));
    }

    [Fact]
    public void UpdateValidator_WhenNationalIdMissing_ShouldFail()
    {
        var validator = new UpdateStudentRequestValidator();
        var request = new UpdateStudentRequest
        {
            FullName = "Test Student",
            NationalId = "",
            ClassSectionId = 1
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(UpdateStudentRequest.NationalId));
    }
}
