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
        }
    }
}
