using BooksPortal.Application.Features.TeacherIssues.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.TeacherIssues.Validators;

public class ProcessTeacherReturnRequestValidator : AbstractValidator<ProcessTeacherReturnRequest>
{
    public ProcessTeacherReturnRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.TeacherIssueItemId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
