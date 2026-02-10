using BooksPortal.Application.Features.Books.DTOs;
using FluentValidation;

namespace BooksPortal.Application.Features.Books.Validators;

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleFor(x => x.ISBN).MaximumLength(20);
        RuleFor(x => x.Author).MaximumLength(300);
        RuleFor(x => x.Publisher).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1900, 2100);
    }
}
