using FluentValidation;
using Service.Helpers.DTOs.GiftCertificates;

namespace Service.Helpers.Validators.GiftCertificates
{
    public class UpdateGiftCertificateDtoValidator : AbstractValidator<UpdateGiftCertificateDto>
    {
        public UpdateGiftCertificateDtoValidator()
        {
            RuleFor(m => m.Code)
                .NotEmpty().WithMessage("Enter the code.");

            RuleFor(m => m.Code)
                .MaximumLength(20).WithMessage("The code can be at most 20 characters.")
                .Matches("^[A-Za-z0-9]+$").WithMessage("The code can have only latin letters and digits.")
                .When(m => !string.IsNullOrWhiteSpace(m.Code));

            RuleFor(m => m.Amount)
                .GreaterThan(0).WithMessage("The amount must be more than 0.")
                .LessThanOrEqualTo(5000).WithMessage("The amount can be at most 5000.");
        }
    }
}
