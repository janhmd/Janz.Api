using Domain.Constants;
using FluentValidation;
using Service.Helpers.DTOs.Products;

namespace Service.Helpers.Validators.Products
{
    // Create və Update validatorları bunu Include ilə istifadə edir
    public class ProductDetailsValidator : AbstractValidator<IProductDetails>
    {
        public ProductDetailsValidator()
        {
            // Köhnə qiymət yalnız endirim varsa mənalıdır: indiki qiymətdən böyük olmalıdır
            RuleFor(m => m.OldPrice)
                .Must((model, oldPrice) => oldPrice is null || oldPrice > model.Price)
                .WithMessage("The old price must be more than the price.")
                .LessThanOrEqualTo(100000).WithMessage("The old price can be at most 100 000.");

            RuleFor(m => m.Badge)
                .Must(badge => string.IsNullOrWhiteSpace(badge) || ProductBadges.All.Contains(badge.Trim()))
                .WithMessage("Choose a badge from the list.");

            RuleFor(m => m.Roast).OptionalText("roast level", 50);
            RuleFor(m => m.Region).OptionalText("region", 50);
            RuleFor(m => m.Processing).OptionalText("processing", 50);
            RuleFor(m => m.Caffeine).OptionalText("caffeine", 20);
            RuleFor(m => m.Brewing).OptionalText("brewing text", 1000);
            RuleFor(m => m.Storage).OptionalText("storage text", 500);
        }
    }
}
