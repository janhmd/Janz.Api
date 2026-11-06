using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Accounts;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // Uğurlu qeydiyyatda token qaytarılmır: emailə 4 rəqəmli kod gedir
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _accountService.RegisterAsync(model);
            if (!result.IsSuccess) return BadRequest(new { isSuccess = false, errors = result.Errors });
            return Ok(new { email = result.Data });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto model)
        {
            return ToAuthResponse(await _accountService.ConfirmEmailAsync(model), unauthorized: false);
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] EmailDto model)
        {
            await _accountService.ResendConfirmationAsync(model);
            return NoContent();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailDto model)
        {
            await _accountService.ForgotPasswordAsync(model);
            return NoContent();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var result = await _accountService.ResetPasswordAsync(model);
            if (!result.IsSuccess) return BadRequest(new { isSuccess = false, errors = result.Errors });
            return Ok(new { email = result.Data });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            return ToAuthResponse(await _accountService.LoginAsync(model), unauthorized: true);
        }

        private static IActionResult ToAuthResponse(ServiceResult<AuthResponseDto> result, bool unauthorized)
        {
            if (!result.IsSuccess)
            {
                if (unauthorized)
                {
                    return new UnauthorizedObjectResult(new { isSuccess = false, errors = result.Errors });
                }

                return new BadRequestObjectResult(new { isSuccess = false, errors = result.Errors });
            }

            return new OkObjectResult(result.Data);
        }
    }
}
