using Application.Abstraction.Consts;
using FluentValidation;

namespace Application.Contracts.Admin;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password should be 8 digits and should contains Lowercase,Uppercase,Number and Special character ");

        RuleFor(x => x.UserFullName)
            .NotEmpty()
            .WithMessage("UserFullName is required")
            .Length(3, 100);


        RuleFor(x => x.UserAddress)
            .NotEmpty()
            .WithMessage("UserAddress is required")
            .Length(3, 100);

        RuleFor(x => x.Role)
            .NotEmpty()
            .NotNull();
    }
}
