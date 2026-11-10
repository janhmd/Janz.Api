using FluentValidation;
using Service.Helpers.DTOs.Contact;

namespace Service.Helpers.Validators.Contact
{
    public class ReplyContactMessageDtoValidator : AbstractValidator<ReplyContactMessageDto>
    {
        public ReplyContactMessageDtoValidator()
        {
            RuleFor(m => m.Reply).RequiredText("reply", 2000);
        }
    }
}
