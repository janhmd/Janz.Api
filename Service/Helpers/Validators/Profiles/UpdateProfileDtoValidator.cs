using FluentValidation;
using Service.Helpers.DTOs.Profiles;

namespace Service.Helpers.Validators.Profiles
{
    public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
    {
        public UpdateProfileDtoValidator()
        {
            RuleFor(m => m.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(80).WithMessage("Full name must be at most 80 characters.");

            RuleFor(m => m.Phone)
                .MaximumLength(30).WithMessage("Phone must be at most 30 characters.")
                .Matches(@"^\+?[0-9 ()-]*$").WithMessage("Phone can contain only digits, spaces, +, ( ) and -.");

            RuleFor(m => m.Nickname)
                .Matches(@"^[A-Za-z0-9_.-]{3,30}$").WithMessage("Only A–Z, 0–9, _, - and . are allowed. The name must be 3 to 30 characters.")
                .When(m => !string.IsNullOrWhiteSpace(m.Nickname));

            RuleFor(m => m.OrganizationName)
                .MaximumLength(200).WithMessage("Organization name must be at most 200 characters.");

            RuleFor(m => m.OrganizationAddress)
                .MaximumLength(300).WithMessage("Legal address must be at most 300 characters.");

            RuleFor(m => m.Voen)
                .Matches(@"^[0-9]{10}$").WithMessage("VOEN must be 10 digits.")
                .When(m => !string.IsNullOrWhiteSpace(m.Voen));

            RuleFor(m => m.TaxNumber)
                .MaximumLength(30).WithMessage("Tax number must be at most 30 characters.");
        }
    }
}
