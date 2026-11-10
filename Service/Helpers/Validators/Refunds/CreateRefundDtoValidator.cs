using FluentValidation;
using Service.Helpers.DTOs.Refunds;

namespace Service.Helpers.Validators.Refunds
{
    public class CreateRefundDtoValidator : AbstractValidator<CreateRefundDto>
    {
        public CreateRefundDtoValidator()
        {
            RuleFor(m => m.Reason).RequiredText("reason", 500);
        }
    }
}
