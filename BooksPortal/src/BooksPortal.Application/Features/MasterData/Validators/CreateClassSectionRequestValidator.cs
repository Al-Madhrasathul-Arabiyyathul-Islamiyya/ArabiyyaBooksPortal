using BooksPortal.Application.Features.MasterData.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.MasterData.Validators;

public class CreateClassSectionRequestValidator : AbstractValidator<CreateClassSectionRequest>
{
    public CreateClassSectionRequestValidator()
    {
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.KeystageId).GreaterThan(0);
        RuleFor(x => x.GradeId).GreaterThan(0);
        RuleFor(x => x.Section).NotEmpty().MaximumLength(10);
    }
}
