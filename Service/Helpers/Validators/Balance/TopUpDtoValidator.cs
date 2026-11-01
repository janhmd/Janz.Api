using FluentValidation;
using Service.Helpers.DTOs.Balance;

namespace Service.Helpers.Validators.Balance
{
    public class TopUpDtoValidator : AbstractValidator<TopUpDto>
    {
        public TopUpDtoValidator()
        {
            RuleFor(m => m.Amount)
                .GreaterThanOrEqualTo(1).WithMessage("The amount must be at least 1 ₼.")
                .LessThanOrEqualTo(5000).WithMessage("The amount must be at most 5000 ₼.")
                .PrecisionScale(18, 2, true).WithMessage("The amount can have at most 2 decimals.");
        }
    }
}
