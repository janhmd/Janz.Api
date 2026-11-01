using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Profiles;
using Service.Helpers.Responses;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        private string UserId
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return ToResponse(await _profileService.GetAsync(UserId));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateProfileDto model)
        {
            return ToResponse(await _profileService.UpdateAsync(UserId, model));
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var result = await _profileService.ChangePasswordAsync(UserId, model);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return ToResponse(result);
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarDto model)
        {
            return ToResponse(await _profileService.UploadAvatarAsync(UserId, model));
        }

        [HttpDelete("avatar")]
        public async Task<IActionResult> RemoveAvatar()
        {
            return ToResponse(await _profileService.RemoveAvatarAsync(UserId));
        }

        private static IActionResult ToResponse<T>(ServiceResult<T> result)
        {
            if (result.IsNotFound)
            {
                return new NotFoundObjectResult(new
                {
                    isSuccess = false,
                    errors = result.Errors
                });
            }

            if (!result.IsSuccess)
            {
                return new BadRequestObjectResult(new
                {
                    isSuccess = false,
                    errors = result.Errors
                });
            }

            return new OkObjectResult(result.Data);
        }
    }
}
