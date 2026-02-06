using BooksPortal.Application.Features.Distribution.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.Distribution.Validators;

public class CreateDistributionSlipRequestValidator : AbstractValidator<CreateDistributionSlipRequest>
{
    public CreateDistributionSlipRequestValidator()
    {
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Term).IsInEnum();
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.ParentId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.BookId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
