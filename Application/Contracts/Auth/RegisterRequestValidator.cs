using Application.Abstraction.Consts;
using FluentValidation;

namespace Application.Contracts.Auth;

public class RegisterrequestValidator : AbstractValidator<Registerrequest>
{

    public RegisterrequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .Matches(RegexPatterns.Password)
            .WithMessage("Password should be 8 digits and should contains Lowercase,Uppercase,Number and Special character ");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("full name  is required")
            .Length(3, 100);


    }

}
