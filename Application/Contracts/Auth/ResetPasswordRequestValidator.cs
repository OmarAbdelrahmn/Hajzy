using Application.Abstraction.Consts;
using FluentValidation;

namespace Application.Contracts.Auth;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{

    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress();

        RuleFor(x => x.Code)
            .NotEmpty();

        RuleFor(x => x.Password)
        .NotEmpty()
            .WithMessage("Password is required")
            .Matches(RegexPatterns.Password)
            .WithMessage("Password should be 8 digits and should contains Lowercase,Uppercase,Number and Special character ");


    }

}
