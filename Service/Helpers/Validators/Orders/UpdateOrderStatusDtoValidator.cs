using Domain.Enums;
using FluentValidation;
using Service.Helpers.DTOs.Orders;

namespace Service.Helpers.Validators.Orders
{
    public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
    {
        public UpdateOrderStatusDtoValidator()
        {
            RuleFor(m => m.Status)
                .NotEmpty().WithMessage("Choose a status.");

            // Yalnız adla qəbul olunur ("Shipped"), rəqəmlə ("2") yox
            RuleFor(m => m.Status)
                .Must(value => Enum.TryParse<OrderStatus>(value, true, out _) && !int.TryParse(value, out _))
                    .WithMessage("This status is not known.")
                .When(m => !string.IsNullOrWhiteSpace(m.Status));
        }
    }
}
