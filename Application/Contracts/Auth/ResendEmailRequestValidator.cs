using FluentValidation;

namespace Application.Contracts.Auth;

public class ResendEmailRequestValidator : AbstractValidator<ResendEmailRequest>
{
    public ResendEmailRequestValidator()
    {
        RuleFor(c => c.Email)
            .EmailAddress()
            .NotEmpty();
    }


}
