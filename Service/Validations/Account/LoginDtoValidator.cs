using FluentValidation;
using Service.DTOs.Account;

namespace Service.Validations.Account;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(m => m.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(m => m.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
