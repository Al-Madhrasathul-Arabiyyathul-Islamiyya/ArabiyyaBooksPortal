using BooksPortal.Application.Features.MasterData.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.MasterData.Validators;

public class CreateAcademicYearRequestValidator : AbstractValidator<CreateAcademicYearRequest>
{
    public CreateAcademicYearRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Year).GreaterThan(2000);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate);
    }
}
