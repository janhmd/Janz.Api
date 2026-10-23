using FluentValidation;
using Service.Helpers.DTOs.Tags;

namespace Service.Helpers.Validators.Tags
{
    public class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
    {
        public CreateTagDtoValidator()
        {
            RuleFor(m => m.Name).RequiredText("name", 100);

            // Slug istəyə bağlıdır; yazılıbsa yalnız kiçik latın hərfləri, rəqəmlər və defis
            RuleFor(m => m.Slug)
                .Matches(SlugHelper.Pattern).WithMessage("The slug can have only small latin letters, numbers and hyphens.")
                .MaximumLength(100).WithMessage("The slug can be at most 100 characters.")
                .When(m => !string.IsNullOrWhiteSpace(m.Slug));
        }
    }
}
