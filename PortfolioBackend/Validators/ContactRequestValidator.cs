using FluentValidation;
using PortfolioBackend.Contacts;

namespace PortfolioBackend.Validators;

public sealed class ContactRequestValidator : FluentValidation.AbstractValidator<ContactRequest>
{
    public ContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .MaximumLength(254).WithMessage("Email address is too long.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(150).WithMessage("Subject must not exceed 150 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MinimumLength(10).WithMessage("Message must be at least 10 characters long.")
            .MaximumLength(3000).WithMessage("Message must not exceed 3000 characters.");

        RuleFor(x => x.HoneypotToken)
            .Empty().WithMessage("Spam detected.");
    }
}