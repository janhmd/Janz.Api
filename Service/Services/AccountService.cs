using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Service.DTOs.Account;
using Service.DTOs.Common;
using Service.Helpers;
using Service.Mappings;
using Service.Services.Interfaces;

namespace Service.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IValidator<RegisterDto> _registerValidator;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly JwtTokenGenerator _jwtTokenGenerator;

    public AccountService(
        UserManager<AppUser> userManager,
        IValidator<RegisterDto> registerValidator,
        IValidator<LoginDto> loginValidator,
        JwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var validation = await _registerValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return ServiceResult<AuthResponseDto>.Fail(validation.Errors.Select(error => error.ErrorMessage));
        }

        if (await _userManager.FindByEmailAsync(dto.Email.Trim()) is not null)
        {
            return ServiceResult<AuthResponseDto>.Fail("This email is already registered.");
        }

        var user = AccountMapper.ToEntity(dto);
        var created = await _userManager.CreateAsync(user, dto.Password);
        if (!created.Succeeded)
        {
            return ServiceResult<AuthResponseDto>.Fail(created.Errors.Select(error => error.Description));
        }

        await _userManager.AddToRoleAsync(user, Roles.User);
        return ServiceResult<AuthResponseDto>.Ok(await BuildResponseAsync(user));
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var validation = await _loginValidator.ValidateAsync(dto);
        if (!validation.IsValid)
        {
            return ServiceResult<AuthResponseDto>.Fail(validation.Errors.Select(error => error.ErrorMessage));
        }

        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return ServiceResult<AuthResponseDto>.Fail("Email or password is wrong.");
        }

        return ServiceResult<AuthResponseDto>.Ok(await BuildResponseAsync(user));
    }

    private async Task<AuthResponseDto> BuildResponseAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? Roles.User;
        var token = _jwtTokenGenerator.CreateToken(user, roles);
        return AccountMapper.ToResponse(user, token, role);
    }
}
