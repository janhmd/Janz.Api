using FluentValidation;
using Service.Helpers.DTOs.Accounts;

namespace Service.Helpers.Validators.Accounts
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(m => m.UserId).NotEmpty().WithMessage("The link is not valid.");
            RuleFor(m => m.Token).NotEmpty().WithMessage("The link is not valid.");

            RuleFor(m => m.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(m => m.RepeatPassword)
                .Equal(m => m.Password).WithMessage("Passwords do not match.");
        }
    }
}
