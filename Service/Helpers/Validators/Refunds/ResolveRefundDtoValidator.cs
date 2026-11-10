using FluentValidation;
using Service.Helpers.DTOs.Refunds;

namespace Service.Helpers.Validators.Refunds
{
    public class ResolveRefundDtoValidator : AbstractValidator<ResolveRefundDto>
    {
        public ResolveRefundDtoValidator()
        {
            RuleFor(m => m.Note).OptionalText("note", 500);

            RuleFor(m => m.Amount)
                .GreaterThan(0).WithMessage("The refund amount must be more than zero.")
                .When(m => m.Amount is not null);
        }
    }
}
