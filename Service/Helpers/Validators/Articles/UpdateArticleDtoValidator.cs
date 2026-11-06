using FluentValidation;
using Service.Helpers.DTOs.Articles;

namespace Service.Helpers.Validators.Articles
{
    public class UpdateArticleDtoValidator : AbstractValidator<UpdateArticleDto>
    {
        public UpdateArticleDtoValidator()
        {
            RuleFor(m => m.Title)
                .NotEmpty().WithMessage("Enter the title.")
                .MaximumLength(200).WithMessage("The title can be at most 200 characters.");

            RuleFor(m => m.Slug)
                .Matches(SlugHelper.Pattern).WithMessage("The slug can have only small latin letters, digits and dashes.")
                .MaximumLength(200).WithMessage("The slug can be at most 200 characters.")
                .When(m => !string.IsNullOrWhiteSpace(m.Slug));

            RuleFor(m => m.Topic)
                .Must(value => ArticleRules.Topics.Contains(value)).WithMessage("Choose a topic from the list.");

            RuleFor(m => m.Lead)
                .NotEmpty().WithMessage("Enter the lead.")
                .MaximumLength(300).WithMessage("The lead can be at most 300 characters.");

            RuleFor(m => m.Body)
                .NotEmpty().WithMessage("Write the article text.");

            RuleForEach(m => m.Tags)
                .Must(value => ArticleRules.Tags.Contains(value)).WithMessage("Choose tags from the list.");

            RuleFor(m => m.PublishedAt)
                .Must(value => value!.Value.ToUniversalTime() <= DateTime.UtcNow.AddDays(1)).WithMessage("The published date can't be in the future.")
                .When(m => m.PublishedAt.HasValue);

            RuleFor(m => m.AuthorName).MaximumLength(100).WithMessage("The author name can be at most 100 characters.");
            RuleFor(m => m.AuthorRole).MaximumLength(100).WithMessage("The author role can be at most 100 characters.");
            RuleFor(m => m.Series).MaximumLength(100).WithMessage("The series can be at most 100 characters.");
        }
    }
}
