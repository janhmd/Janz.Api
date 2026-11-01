using FluentValidation;
using Service.Helpers.DTOs.Profiles;

namespace Service.Helpers.Validators.Profiles
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(m => m.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(m => m.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters.")
                .NotEqual(m => m.CurrentPassword).WithMessage("The new password must be different from the current one.");
        }
    }
}
