using BooksPortal.Application.Features.Settings.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.Settings.Validators;

public class UpdateSlipTemplateSettingRequestValidator : AbstractValidator<UpdateSlipTemplateSettingRequest>
{
    public UpdateSlipTemplateSettingRequestValidator()
    {
        RuleFor(x => x.Value).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
