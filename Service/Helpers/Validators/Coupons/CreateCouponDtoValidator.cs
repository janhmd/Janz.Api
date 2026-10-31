using Domain.Enums;
using FluentValidation;
using Service.Helpers.DTOs.Coupons;

namespace Service.Helpers.Validators.Coupons
{
    public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
    {
        public CreateCouponDtoValidator()
        {
            RuleFor(m => m.Code)
                .NotEmpty().WithMessage("Enter the code.");

            // Kod yalnız latın hərfləri və rəqəmlərdən olur (servis böyük hərflə saxlayır)
            RuleFor(m => m.Code)
                .MaximumLength(20).WithMessage("The code can be at most 20 characters.")
                .Matches("^[A-Za-z0-9]+$").WithMessage("The code can have only latin letters and digits.")
                .When(m => !string.IsNullOrWhiteSpace(m.Code));

            // Yalnız adla qəbul olunur ("Percent"), rəqəmlə ("0") yox
            RuleFor(m => m.Type)
                .Must(value => Enum.TryParse<CouponType>(value, true, out _) && !int.TryParse(value, out _))
                .WithMessage("Choose the type: Percent or Fixed.");

            RuleFor(m => m.Value)
                .GreaterThan(0).WithMessage("The value must be more than 0.")
                .LessThanOrEqualTo(10000).WithMessage("The value can be at most 10 000.");

            // Faiz 100-dən çox ola bilməz
            RuleFor(m => m.Value)
                .LessThanOrEqualTo(100).WithMessage("The percent can be at most 100.")
                .When(m => string.Equals(m.Type, nameof(CouponType.Percent), StringComparison.OrdinalIgnoreCase));

            RuleFor(m => m.MinOrder)
                .GreaterThanOrEqualTo(0).WithMessage("The min order can't be negative.")
                .LessThanOrEqualTo(100000).WithMessage("The min order can be at most 100 000.");

            RuleFor(m => m.UsageLimit)
                .GreaterThan(0).WithMessage("The usage limit must be at least 1.")
                .When(m => m.UsageLimit is not null);

            RuleFor(m => m.EndsAt)
                .Must((model, endsAt) => endsAt is null || model.StartsAt is null || endsAt >= model.StartsAt)
                .WithMessage("The end date must be after the start date.");
        }
    }
}
