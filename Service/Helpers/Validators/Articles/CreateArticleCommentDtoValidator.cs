using FluentValidation;
using Service.Helpers.DTOs.Articles;

namespace Service.Helpers.Validators.Articles
{
    public class CreateArticleCommentDtoValidator : AbstractValidator<CreateArticleCommentDto>
    {
        public CreateArticleCommentDtoValidator()
        {
            RuleFor(m => m.Text)
                .NotEmpty().WithMessage("Write a comment.")
                .MaximumLength(1000).WithMessage("The comment can be at most 1000 characters.");
        }
    }
}
