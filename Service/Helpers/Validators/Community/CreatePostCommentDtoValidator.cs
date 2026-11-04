using FluentValidation;
using Service.Helpers.DTOs.Community;

namespace Service.Helpers.Validators.Community
{
    public class CreatePostCommentDtoValidator : AbstractValidator<CreatePostCommentDto>
    {
        public CreatePostCommentDtoValidator()
        {
            RuleFor(m => m.Text)
                .MaximumLength(1000).WithMessage("The comment can be at most 1000 characters.");

            RuleFor(m => m)
                .Must(m => !string.IsNullOrWhiteSpace(m.Text) || m.Image is not null).WithMessage("Write a comment or add a photo.");
        }
    }
}
