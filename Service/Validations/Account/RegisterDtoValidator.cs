using FluentValidation;
using Service.DTOs.Account;

namespace Service.Validations.Account;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(m => m.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(80).WithMessage("Full name must be at most 80 characters.");

        RuleFor(m => m.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(m => m.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(m => m.RepeatPassword)
            .Equal(m => m.Password).WithMessage("Passwords do not match.");
    }
}
