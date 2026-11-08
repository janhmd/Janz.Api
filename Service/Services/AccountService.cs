using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
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
        private readonly IValidator<ResetPasswordDto> _resetValidator;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AccountService(UserManager<AppUser> userManager,
                              IValidator<RegisterDto> registerValidator,
                              IValidator<LoginDto> loginValidator,
                              IValidator<ResetPasswordDto> resetValidator,
                              JwtTokenGenerator jwtTokenGenerator,
                              IEmailService emailService,
                              IConfiguration configuration)
        {
            _userManager = userManager;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _resetValidator = resetValidator;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _configuration = configuration;
        }

        private const string CodeProvider = "Janz";
        private const string CodeName = "EmailCode";

        public async Task<ServiceResult<string>> RegisterAsync(RegisterDto model)
        {
            var validation = await _registerValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<string>.Fail(validation.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            // Kodu yazıb təsdiqləməyən qeydiyyat tamamlanmış sayılmır: eyni email yenidən qeydiyyatdan keçə bilər
            var existing = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (existing is not null)
            {
                if (existing.EmailConfirmed)
                {
                    return ServiceResult<string>.Fail("This email is already registered.");
                }

                await _userManager.DeleteAsync(existing);
            }

            var user = ToEntity(model);
            var created = await _userManager.CreateAsync(user, model.Password);
            if (!created.Succeeded)
            {
                return ServiceResult<string>.Fail(created.Errors.Select(e => e.Description).ToArray());
            }

            await _userManager.AddToRoleAsync(user, Roles.User);
            await SendConfirmationAsync(user);
            return ServiceResult<string>.Ok(user.Email!);
        }

        // Emailə gedən 4 rəqəmli kod düzdürsə email təsdiqlənir (qeydiyyat tamamlanır) və istifadəçi dərhal daxil olur
        public async Task<ServiceResult<AuthResponseDto>> ConfirmEmailAsync(ConfirmEmailDto model)
        {
            var user = await FindByEmailAsync(model.Email);
            if (user is null)
            {
                return ServiceResult<AuthResponseDto>.Fail("The code is not valid. Request a new one.");
            }

            if (!user.EmailConfirmed)
            {
                var stored = await _userManager.GetAuthenticationTokenAsync(user, CodeProvider, CodeName);
                var (result, updated) = EmailCode.Check(stored, user.Id, model.Code, DateTime.UtcNow);
                if (result == EmailCodeResult.Wrong)
                {
                    await _userManager.SetAuthenticationTokenAsync(user, CodeProvider, CodeName, updated);
                    return ServiceResult<AuthResponseDto>.Fail("The code is wrong. Check it and try again.");
                }

                if (result != EmailCodeResult.Valid)
                {
                    await _userManager.RemoveAuthenticationTokenAsync(user, CodeProvider, CodeName);
                    return ServiceResult<AuthResponseDto>.Fail(result == EmailCodeResult.Locked
                        ? "Too many wrong codes. Request a new one."
                        : "The code has expired. Request a new one.");
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmed = await _userManager.ConfirmEmailAsync(user, token);
                if (!confirmed.Succeeded)
                {
                    return ServiceResult<AuthResponseDto>.Fail("The code is not valid. Request a new one.");
                }

                await _userManager.RemoveAuthenticationTokenAsync(user, CodeProvider, CodeName);
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return ServiceResult<AuthResponseDto>.Fail("Your account is blocked. Please contact support.");
            }

            return ServiceResult<AuthResponseDto>.Ok(await BuildResponseAsync(user));
        }

        // Email qeydiyyatda olub-olmadığı bildirilmir (başqasının emailini yoxlamaq olmasın)
        public async Task ResendConfirmationAsync(EmailDto model)
        {
            var user = await FindByEmailAsync(model.Email);
            if (user is not null && !user.EmailConfirmed)
            {
                await SendConfirmationAsync(user);
            }
        }

        public async Task ForgotPasswordAsync(EmailDto model)
        {
            var user = await FindByEmailAsync(model.Email);
            if (user is null) return;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = MvcLink("Account/ResetPassword", user.Id, token);
            await _emailService.SendAsync(user.Email!, "Reset your Janz password", Letter(
                user.Name,
                "We got a request to reset your password. Open the link below to choose a new one. If it wasn't you, ignore this email.",
                "Choose a new password",
                link));
        }

        public async Task<ServiceResult<string>> ResetPasswordAsync(ResetPasswordDto model)
        {
            var validation = await _resetValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return ServiceResult<string>.Fail(validation.Errors.Select(e => e.ErrorMessage).Distinct().ToArray());
            }

            var user = await _userManager.FindByIdAsync(model.UserId!);
            var token = Decode(model.Token);
            if (user is null || token is null)
            {
                return ServiceResult<string>.Fail("The link is not valid.");
            }

            if (!string.Equals(user.Email, model.Email!.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<string>.Fail("The email does not match this link.");
            }

            var result = await _userManager.ResetPasswordAsync(user, token, model.Password!);
            if (!result.Succeeded)
            {
                return ServiceResult<string>.Fail("The link is not valid or has expired. Request a new one.");
            }

            // Linkə klik edib şifrəni dəyişən öz emailini də təsdiqləmiş olur
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            return ServiceResult<string>.Ok(user.Email!);
        }

        private async Task SendConfirmationAsync(AppUser user)
        {
            var code = EmailCode.NewCode();
            await _userManager.SetAuthenticationTokenAsync(user, CodeProvider, CodeName, EmailCode.Pack(user.Id, code, DateTime.UtcNow));
            await _emailService.SendAsync(user.Email!, "Your Janz code", CodeLetter(user.Name, code));
        }

        private async Task<AppUser?> FindByEmailAsync(string? email)
        {
            return string.IsNullOrWhiteSpace(email) ? null : await _userManager.FindByEmailAsync(email.Trim());
        }

        // Linklər MVC saytına aparır (appsettings -> Frontend:MvcBaseUrl)
        private string MvcLink(string path, string userId, string token)
        {
            var baseUrl = (_configuration["Frontend:MvcBaseUrl"] ?? "https://localhost:7047").TrimEnd('/');
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            return $"{baseUrl}/{path}?userId={Uri.EscapeDataString(userId)}&token={encoded}";
        }

        private static string? Decode(string? token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            try
            {
                return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private static string CodeLetter(string? name, string code)
        {
            var hello = string.IsNullOrWhiteSpace(name) ? "Hello," : $"Hello, {WebUtility.HtmlEncode(name)}!";
            return $@"<div style=""font-family:Arial,sans-serif;max-width:480px;margin:0 auto;padding:24px;color:#111"">
<h2 style=""margin:0 0 16px"">Janz</h2>
<p>{hello}</p>
<p>Thanks for signing up. Enter this code on the site to finish the registration:</p>
<p style=""margin:24px 0;font-size:32px;font-weight:bold;letter-spacing:8px"">{code}</p>
<p style=""color:#666;font-size:12px"">The code works for 10 minutes. If you did not sign up, just ignore this email.</p>
</div>";
        }

        private static string Letter(string? name, string text, string button, string link)
        {
            var hello = string.IsNullOrWhiteSpace(name) ? "Hello," : $"Hello, {WebUtility.HtmlEncode(name)}!";
            return $@"<div style=""font-family:Arial,sans-serif;max-width:480px;margin:0 auto;padding:24px;color:#111"">
<h2 style=""margin:0 0 16px"">Janz</h2>
<p>{hello}</p>
<p>{WebUtility.HtmlEncode(text)}</p>
<p style=""margin:24px 0""><a href=""{link}"" style=""background:#111;color:#fff;padding:12px 20px;border-radius:999px;text-decoration:none"">{WebUtility.HtmlEncode(button)}</a></p>
<p style=""color:#666;font-size:12px"">If the button doesn't work, open this link: {link}</p>
</div>";
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

            if (!user.EmailConfirmed)
            {
                return ServiceResult<AuthResponseDto>.Fail("Confirm your email first. We sent you a code when you signed up.");
            }

            // Admin paneldən bloklanmış hesab daxil ola bilmir
            if (await _userManager.IsLockedOutAsync(user))
            {
                return ServiceResult<AuthResponseDto>.Fail("Your account is blocked. Please contact support.");
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
