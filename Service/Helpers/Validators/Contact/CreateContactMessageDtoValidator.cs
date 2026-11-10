using Domain.Constants;
using FluentValidation;
using Service.Helpers.DTOs.Contact;

namespace Service.Helpers.Validators.Contact
{
    public class CreateContactMessageDtoValidator : AbstractValidator<CreateContactMessageDto>
    {
        public CreateContactMessageDtoValidator()
        {
            RuleFor(m => m.Name).RequiredText("name", 80);

            RuleFor(m => m.Email)
                .NotEmpty().WithMessage("Enter your email.")
                .EmailAddress().WithMessage("Email is not valid.")
                .MaximumLength(150).WithMessage("The email can be at most 150 characters.");

            RuleFor(m => m.Topic)
                .Must(value => value is not null && ContactTopics.All.Contains(value))
                .WithMessage("Choose what your message is about.");

            RuleFor(m => m.Message)
                .Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Write your message.")
                .Must(value => string.IsNullOrWhiteSpace(value) || value.Trim().Length >= 10).WithMessage("The message is too short. Please add a few more details.")
                .Must(value => string.IsNullOrWhiteSpace(value) || value.Trim().Length <= 2000).WithMessage("The message can be at most 2000 characters.");
        }
    }
}
