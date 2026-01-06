using Application.Abstraction.Consts;
using FluentValidation;

namespace Application.Contracts.User;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(i => i.CurrentPassword)
            .NotEmpty();

    }

}
