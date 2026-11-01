using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Service.Helpers.DTOs.Profiles;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class ProfileService : IProfileService
    {
        private const string AvatarFolder = "avatars";

        private readonly UserManager<AppUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IValidator<UpdateProfileDto> _updateValidator;
        private readonly IValidator<ChangePasswordDto> _passwordValidator;

        public ProfileService(UserManager<AppUser> userManager,
                              IFileService fileService,
                              IValidator<UpdateProfileDto> updateValidator,
                              IValidator<ChangePasswordDto> passwordValidator)
        {
            _userManager = userManager;
            _fileService = fileService;
            _updateValidator = updateValidator;
            _passwordValidator = passwordValidator;
        }

        public async Task<ServiceResult<ProfileDto>> GetAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return ServiceResult<ProfileDto>.NotFound();
            }

            return ServiceResult<ProfileDto>.Ok(ToDto(user));
        }

        public async Task<ServiceResult<ProfileDto>> UpdateAsync(string userId, UpdateProfileDto model)
        {
            var validation = await _updateValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<ProfileDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return ServiceResult<ProfileDto>.NotFound();
            }

            // Ləqəb unikal olmalıdır (böyük-kiçik hərf fərq etmir)
            var nickname = Clean(model.Nickname);
            if (nickname is not null)
            {
                var taken = _userManager.Users.Any(m => m.Id != userId && m.Nickname != null && m.Nickname.ToLower() == nickname.ToLower());
                if (taken)
                {
                    return ServiceResult<ProfileDto>.Fail("This public username is already taken.");
                }
            }

            var parts = model.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            user.Name = parts.Length > 0 ? parts[0] : string.Empty;
            user.Surname = parts.Length > 1 ? parts[1] : string.Empty;
            user.PhoneNumber = Clean(model.Phone);
            user.Nickname = nickname;
            user.OrganizationName = Clean(model.OrganizationName);
            user.OrganizationAddress = Clean(model.OrganizationAddress);
            user.Voen = Clean(model.Voen);
            user.TaxNumber = Clean(model.TaxNumber);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return ServiceResult<ProfileDto>.Fail(result.Errors.Select(e => e.Description).ToArray());
            }

            return ServiceResult<ProfileDto>.Ok(ToDto(user));
        }

        public async Task<ServiceResult<bool>> ChangePasswordAsync(string userId, ChangePasswordDto model)
        {
            var validation = await _passwordValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<bool>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return ServiceResult<bool>.NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                // Identity "Incorrect password." qaytarır, istifadəçiyə aydın mesaj göstərilir
                var errors = result.Errors.Select(e => e.Code == "PasswordMismatch" ? "The current password is wrong." : e.Description).ToArray();
                return ServiceResult<bool>.Fail(errors);
            }

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<ProfileDto>> UploadAvatarAsync(string userId, UploadAvatarDto model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return ServiceResult<ProfileDto>.NotFound();
            }

            var oldPath = user.AvatarPath;
            user.AvatarPath = await _fileService.UploadImageAsync(model.Image!, AvatarFolder);
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(oldPath))
            {
                await _fileService.DeleteFileAsync(oldPath);
            }

            return ServiceResult<ProfileDto>.Ok(ToDto(user));
        }

        public async Task<ServiceResult<ProfileDto>> RemoveAvatarAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return ServiceResult<ProfileDto>.NotFound();
            }

            if (!string.IsNullOrEmpty(user.AvatarPath))
            {
                await _fileService.DeleteFileAsync(user.AvatarPath);
                user.AvatarPath = null;
                await _userManager.UpdateAsync(user);
            }

            return ServiceResult<ProfileDto>.Ok(ToDto(user));
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static ProfileDto ToDto(AppUser user)
        {
            return new ProfileDto
            {
                FullName = string.Join(" ", new[] { user.Name, user.Surname }.Where(part => !string.IsNullOrWhiteSpace(part))),
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                Nickname = user.Nickname,
                AvatarPath = user.AvatarPath,
                OrganizationName = user.OrganizationName,
                OrganizationAddress = user.OrganizationAddress,
                Voen = user.Voen,
                TaxNumber = user.TaxNumber
            };
        }
    }
}
