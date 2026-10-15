using FluentValidation;
using Service.Helpers.DTOs.Addresses;

namespace Service.Helpers.Validators.Addresses
{
    public class UpdateAddressDtoValidator : AbstractValidator<UpdateAddressDto>
    {
        public UpdateAddressDtoValidator()
        {
            RuleFor(m => m.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(80).WithMessage("Full name must be at most 80 characters.");

            RuleFor(m => m.Phone)
                .NotEmpty().WithMessage("Phone is required.")
                .MaximumLength(30).WithMessage("Phone must be at most 30 characters.");

            RuleFor(m => m.Line)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(200).WithMessage("Address must be at most 200 characters.");

            RuleFor(m => m.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(80).WithMessage("City must be at most 80 characters.");

            RuleFor(m => m.PostalCode)
                .NotEmpty().WithMessage("Postal code is required.")
                .MaximumLength(20).WithMessage("Postal code must be at most 20 characters.");
        }
    }
}
