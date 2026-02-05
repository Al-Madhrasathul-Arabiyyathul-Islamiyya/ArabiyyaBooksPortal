using BooksPortal.Application.Features.Books.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.Books.Validators;

public class AddStockRequestValidator : AbstractValidator<AddStockRequest>
{
    public AddStockRequestValidator()
    {
        RuleFor(x => x.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
