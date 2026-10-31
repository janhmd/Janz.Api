using FluentValidation;
using Service.Helpers.DTOs.Orders;

namespace Service.Helpers.Validators.Orders
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(m => m.AddressId)
                .GreaterThan(0).WithMessage("Choose a delivery address.");

            RuleFor(m => m.CouponCode)
                .MaximumLength(20).WithMessage("This promo code is not valid.");
        }
    }
}
