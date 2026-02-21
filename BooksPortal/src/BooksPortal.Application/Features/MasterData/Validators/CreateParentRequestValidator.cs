using BooksPortal.Application.Features.MasterData.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.MasterData.Validators;

public class CreateParentRequestValidator : AbstractValidator<CreateParentRequest>
{
    public CreateParentRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NationalId).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Relationship).MaximumLength(50);
    }
}
