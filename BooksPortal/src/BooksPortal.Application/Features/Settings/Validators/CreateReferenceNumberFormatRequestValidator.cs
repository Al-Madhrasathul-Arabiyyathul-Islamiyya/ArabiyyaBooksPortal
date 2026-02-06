using BooksPortal.Application.Features.Settings.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.Settings.Validators;

public class CreateReferenceNumberFormatRequestValidator : AbstractValidator<CreateReferenceNumberFormatRequest>
{
    public CreateReferenceNumberFormatRequestValidator()
    {
        RuleFor(x => x.SlipType).IsInEnum();
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.FormatTemplate)
            .NotEmpty()
            .MaximumLength(100)
            .Must(ContainAutonum).WithMessage("FormatTemplate must contain {autonum} token.");
        RuleFor(x => x.PaddingWidth).InclusiveBetween(1, 10);
    }

    private static bool ContainAutonum(string template)
        => template.Contains("{autonum}", StringComparison.OrdinalIgnoreCase);
}
