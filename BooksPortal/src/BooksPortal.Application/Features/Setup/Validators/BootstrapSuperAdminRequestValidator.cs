using BooksPortal.Application.Features.Setup.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.Setup.Validators;

public class BootstrapSuperAdminRequestValidator : AbstractValidator<BootstrapSuperAdminRequest>
{
    public BootstrapSuperAdminRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NationalId).MaximumLength(50);
        RuleFor(x => x.Designation).MaximumLength(100);
    }
}

