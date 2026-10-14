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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            return ToAuthResponse(await _accountService.RegisterAsync(model), unauthorized: false);
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
