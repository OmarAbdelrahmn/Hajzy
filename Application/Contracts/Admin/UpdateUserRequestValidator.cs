using FluentValidation;

namespace Application.Contracts.Admin;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.UserFullName)
            .NotEmpty()
            .WithMessage("UserFullName is required")
            .Length(3, 100);


        RuleFor(x => x.UserAddress)
            .NotEmpty()
            .WithMessage("User address is required")
            .Length(3, 100);

        RuleFor(x => x.Role)
            .NotEmpty()
            .NotNull();
    }
}
