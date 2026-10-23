using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Service.Helpers;
using Service.Helpers.DTOs.Accounts;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly JwtTokenGenerator _jwtTokenGenerator;

        public AccountService(UserManager<AppUser> userManager,
                              IValidator<RegisterDto> registerValidator,
                              IValidator<LoginDto> loginValidator,
                              JwtTokenGenerator jwtTokenGenerator)
        {
            _userManager = userManager;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto model)
        {
            var validation = await _registerValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<AuthResponseDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            if (await _userManager.FindByEmailAsync(model.Email.Trim()) is not null)
            {
                return ServiceResult<AuthResponseDto>.Fail("This email is already registered.");
            }

            var user = ToEntity(model);
            var created = await _userManager.CreateAsync(user, model.Password);
            if (!created.Succeeded)
            {
                return ServiceResult<AuthResponseDto>.Fail(created.Errors.Select(e => e.Description).ToArray());
            }

            await _userManager.AddToRoleAsync(user, Roles.User);
            return ServiceResult<AuthResponseDto>.Ok(await BuildResponseAsync(user));
        }

        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto model)
        {
            var validation = await _loginValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<AuthResponseDto>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return ServiceResult<AuthResponseDto>.Fail("Email or password is wrong.");
            }

            return ServiceResult<AuthResponseDto>.Ok(await BuildResponseAsync(user));
        }

        private async Task<AuthResponseDto> BuildResponseAsync(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = Roles.Highest(roles);
            var token = _jwtTokenGenerator.CreateToken(user, roles);
            return ToDto(user, token, role);
        }

        private static AppUser ToEntity(RegisterDto model)
        {
            SplitName(model.FullName, out var name, out var surname);

            return new AppUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                Name = name,
                Surname = surname
            };
        }

        private static AuthResponseDto ToDto(AppUser user, string token, string role)
        {
            var dto = new AuthResponseDto();
            Fill(dto, user, token, role);
            return dto;
        }

        private static void Fill(AuthResponseDto dto, AppUser user, string token, string role)
        {
            var fullName = string.Join(" ", new[] { user.Name, user.Surname }.Where(part => !string.IsNullOrWhiteSpace(part)));
            dto.Token = token;
            dto.Email = user.Email ?? string.Empty;
            dto.FullName = fullName;
            dto.Role = role;
        }

        private static void SplitName(string fullName, out string name, out string surname)
        {
            var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            name = parts.Length > 0 ? parts[0] : string.Empty;
            surname = parts.Length > 1 ? parts[1] : string.Empty;
        }
    }
}
