using Domain.Constants;
using FluentValidation;
using Service.Helpers.DTOs.Users;

namespace Service.Helpers.Validators.Users
{
    public class UpdateUserRoleDtoValidator : AbstractValidator<UpdateUserRoleDto>
    {
        public UpdateUserRoleDtoValidator()
        {
            RuleFor(m => m.Role)
                .NotEmpty().WithMessage("Choose a role.");

            RuleFor(m => m.Role)
                .Must(value => Roles.All.Contains(value))
                    .WithMessage("This role is not known.")
                .When(m => !string.IsNullOrWhiteSpace(m.Role));
        }
    }
}
