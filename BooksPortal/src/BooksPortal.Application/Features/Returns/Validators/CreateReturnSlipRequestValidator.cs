using BooksPortal.Application.Features.Returns.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.Returns.Validators;

public class CreateReturnSlipRequestValidator : AbstractValidator<CreateReturnSlipRequest>
{
    public CreateReturnSlipRequestValidator()
    {
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.ReturnedById).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.BookId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.Condition).IsInEnum();
        });
    }
}
