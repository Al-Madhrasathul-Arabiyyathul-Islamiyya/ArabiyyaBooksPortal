using BooksPortal.Application.Features.TeacherIssues.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.TeacherIssues.Validators;

public class CreateTeacherIssueRequestValidator : AbstractValidator<CreateTeacherIssueRequest>
{
    public CreateTeacherIssueRequestValidator()
    {
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.TeacherId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.BookId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
