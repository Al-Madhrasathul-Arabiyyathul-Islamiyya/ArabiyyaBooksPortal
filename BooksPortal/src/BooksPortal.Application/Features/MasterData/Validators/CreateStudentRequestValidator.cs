using BooksPortal.Application.Features.MasterData.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.MasterData.Validators;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequest>
{
    public CreateStudentRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IndexNo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ClassSectionId).GreaterThan(0);
    }
}
