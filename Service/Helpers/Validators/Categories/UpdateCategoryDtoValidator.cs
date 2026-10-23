using FluentValidation;
using Service.Helpers.DTOs.Categories;

namespace Service.Helpers.Validators.Categories
{
    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryDtoValidator()
        {
            RuleFor(m => m.Name).RequiredText("name", 100);

            // Slug istəyə bağlıdır; yazılıbsa yalnız kiçik latın hərfləri, rəqəmlər və defis
            RuleFor(m => m.Slug)
                .Matches(SlugHelper.Pattern).WithMessage("The slug can have only small latin letters, numbers and hyphens.")
                .MaximumLength(50).WithMessage("The slug can be at most 50 characters.")
                .When(m => !string.IsNullOrWhiteSpace(m.Slug));
        }
    }
}
