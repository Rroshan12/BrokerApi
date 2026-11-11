using FluentValidation;

namespace HouseBroker.Application.Auth;

/// <summary>Sign-up rules. Identity enforces the full password policy on top of these.</summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AccountType).IsInEnum();
        RuleFor(x => x.PhoneNumber).MaximumLength(20).Matches(@"^\+?[0-9 \-]{7,20}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number may contain digits, spaces, dashes and a leading +.");
        RuleFor(x => x.PhoneNumber).NotEmpty().When(x => x.AccountType == AccountType.Broker)
            .WithMessage("Brokers must provide a phone number so seekers can contact them.");
        RuleFor(x => x.AgencyName).MaximumLength(150);
    }
}
