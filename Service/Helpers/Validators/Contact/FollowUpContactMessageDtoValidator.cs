using FluentValidation;
using Service.Helpers.DTOs.Contact;

namespace Service.Helpers.Validators.Contact
{
    public class FollowUpContactMessageDtoValidator : AbstractValidator<FollowUpContactMessageDto>
    {
        public FollowUpContactMessageDtoValidator()
        {
            RuleFor(m => m.Text).RequiredText("message", 2000);
        }
    }
}
