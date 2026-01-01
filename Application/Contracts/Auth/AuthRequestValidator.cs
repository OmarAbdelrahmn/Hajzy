using Application.Abstraction.Consts;
using FluentValidation;

namespace Application.Contracts.Auth;

public class AuthRequestValidator : AbstractValidator<AuthRequest>
{

    public AuthRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .Length(8, 50)
            .Matches(RegexPatterns.Password)
            .WithMessage("Password should be 8 digits and should contains Lowercase,Uppercase,Number and Special character ");
    }
}
