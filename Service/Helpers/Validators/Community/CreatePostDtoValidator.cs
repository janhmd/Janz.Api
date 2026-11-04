using FluentValidation;
using Service.Helpers.DTOs.Community;

namespace Service.Helpers.Validators.Community
{
    public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
    {
        public CreatePostDtoValidator()
        {
            RuleFor(m => m.Text).RequiredText("post text", 2000);

            RuleFor(m => m.Images)
                .Must(list => list.Count <= CommunityRules.MaxImages).WithMessage($"Add at most {CommunityRules.MaxImages} photos.");

            RuleForEach(m => m.Tags)
                .Must(value => CommunityRules.Tags.Contains(value)).WithMessage("Choose tags from the list.");

            RuleFor(m => m.Tags)
                .Must(list => list.Distinct().Count() <= 5).WithMessage("Add at most 5 tags.");

            RuleFor(m => m.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rate the coffee from 1 to 5.")
                .When(m => m.Rating.HasValue);

            RuleFor(m => m.Brew)
                .Must(value => CommunityRules.BrewMethods.Contains(value!)).WithMessage("Choose a brewing method from the list.")
                .When(m => !string.IsNullOrWhiteSpace(m.Brew));

            RuleFor(m => m.Recipe).MaximumLength(300).WithMessage("The recipe can be at most 300 characters.");
            RuleFor(m => m.Dose).MaximumLength(30).WithMessage("The coffee amount can be at most 30 characters.");
            RuleFor(m => m.Water).MaximumLength(30).WithMessage("The water amount can be at most 30 characters.");
            RuleFor(m => m.Grind).MaximumLength(30).WithMessage("The grind can be at most 30 characters.");
            RuleFor(m => m.Time).MaximumLength(30).WithMessage("The time can be at most 30 characters.");
        }
    }
}
