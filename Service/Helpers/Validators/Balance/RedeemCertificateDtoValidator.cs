using FluentValidation;
using Service.Helpers.DTOs.Balance;

namespace Service.Helpers.Validators.Balance
{
    public class RedeemCertificateDtoValidator : AbstractValidator<RedeemCertificateDto>
    {
        public RedeemCertificateDtoValidator()
        {
            RuleFor(m => m.Code)
                .NotEmpty().WithMessage("Enter the certificate code.")
                .MaximumLength(20).WithMessage("The certificate code is not valid.");
        }
    }
}
