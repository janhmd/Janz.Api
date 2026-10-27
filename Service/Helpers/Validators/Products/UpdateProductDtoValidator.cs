using Domain.Constants;
using FluentValidation;
using Service.Helpers.DTOs.Products;

namespace Service.Helpers.Validators.Products
{
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(m => m.Name).RequiredText("name", 150);
            RuleFor(m => m.Description).RequiredText("description", 500);

            // Variant varsa qiymət və stok variantlarda yoxlanılır (ProductVariantFormValidator)
            When(m => !m.HasVariants, () =>
            {
                RuleFor(m => m.Price)
                    .GreaterThan(0).WithMessage("The price must be more than 0.")
                    .LessThanOrEqualTo(100000).WithMessage("The price can be at most 100 000.");

                RuleFor(m => m.Stock)
                    .GreaterThanOrEqualTo(0).WithMessage("The stock can't be negative.")
                    .LessThanOrEqualTo(100000).WithMessage("The stock can be at most 100 000.");
            });

            RuleFor(m => m.CategoryId)
                .GreaterThan(0).WithMessage("Choose a category.");

            // Köhnə qiymət yalnız endirim varsa mənalıdır: indiki qiymətdən böyük olmalıdır
            RuleFor(m => m.OldPrice)
                .Must((model, oldPrice) => oldPrice is null || oldPrice > model.Price)
                .WithMessage("The old price must be more than the price.")
                .LessThanOrEqualTo(100000).WithMessage("The old price can be at most 100 000.")
                .When(m => !m.HasVariants);

            RuleFor(m => m.Badge)
                .Must(badge => string.IsNullOrWhiteSpace(badge) || ProductBadges.All.Contains(badge.Trim()))
                .WithMessage("Choose a badge from the list.");

            RuleFor(m => m.Roast).OptionalText("roast level", 50);
            RuleFor(m => m.Region).OptionalText("region", 50);
            RuleFor(m => m.Processing).OptionalText("processing", 50);
            RuleFor(m => m.Caffeine).OptionalText("caffeine", 20);
            RuleFor(m => m.Brewing).OptionalText("brewing text", 1000);
            RuleFor(m => m.Storage).OptionalText("storage text", 500);

            RuleFor(m => m.Use).OptionalText("use", 50);
            RuleFor(m => m.Taste).OptionalText("taste", 60);
            RuleFor(m => m.Notes).OptionalText("notes", 150);

            RuleFor(m => m.Density)
                .InclusiveBetween(0, 100).WithMessage("The density must be from 0 to 100.")
                .When(m => m.Density is not null);
            RuleFor(m => m.Acidity)
                .InclusiveBetween(0, 100).WithMessage("The acidity must be from 0 to 100.")
                .When(m => m.Acidity is not null);

            RuleFor(m => m.TasteGroup)
                .Must(group => string.IsNullOrWhiteSpace(group) || TasteGroups.All.Contains(group.Trim()))
                .WithMessage("Choose a taste group from the list.");

            RuleFor(m => m.BrewMethods)
                .Must(list => list.All(m => BrewMethods.All.Contains(m)))
                .WithMessage("Choose brewing methods from the list.");

            RuleFor(m => m.Variants)
                .Must(list => list.Count <= ProductRules.MaxVariants)
                .WithMessage($"A product can have at most {ProductRules.MaxVariants} variants.")
                // Eyni paket və ölçü iki dəfə ola bilməz (məs. iki dəfə "Pack · 250 g")
                .Must(list => list.Select(m => ((m.Format ?? "").Trim().ToLower(), (m.Size ?? "").Trim().ToLower())).Distinct().Count() == list.Count)
                .WithMessage("Two variants have the same pack and size.");

            RuleForEach(m => m.Variants).SetValidator(new ProductVariantFormValidator());

            RuleFor(m => m.Gallery)
                .Must(list => list.Count <= ProductRules.MaxGallery)
                .WithMessage($"The gallery can have at most {ProductRules.MaxGallery} images.");
        }
    }
}
