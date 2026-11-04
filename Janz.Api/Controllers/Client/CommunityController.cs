using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Community;
using Service.Services.Interfaces;
using System.Security.Claims;

namespace Janz.Api.Controllers.Client
{
    // Community profilləri, abunəliklər və blacklist
    [Route("api/community")]
    [ApiController]
    public class CommunityController : ControllerBase
    {
        private readonly ICommunityProfileService _profileService;

        public CommunityController(ICommunityProfileService profileService)
        {
            _profileService = profileService;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet("profiles/{key}")]
        public async Task<IActionResult> GetProfile([FromRoute] string key)
        {
            return Ok(await _profileService.GetAsync(key, UserId));
        }

        [HttpGet("profiles/{key}/followers")]
        public async Task<IActionResult> GetFollowers([FromRoute] string key)
        {
            return Ok(await _profileService.GetFollowersAsync(key, UserId));
        }

        [HttpGet("profiles/{key}/following")]
        public async Task<IActionResult> GetFollowing([FromRoute] string key)
        {
            return Ok(await _profileService.GetFollowingAsync(key, UserId));
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMine([FromBody] UpdateCommunityProfileDto request)
        {
            return Ok(await _profileService.UpdateMineAsync(UserId!, request));
        }

        [HttpPost("follow/{targetId}")]
        [Authorize]
        public async Task<IActionResult> Follow([FromRoute] string targetId)
        {
            await _profileService.FollowAsync(UserId!, targetId);
            return NoContent();
        }

        [HttpDelete("follow/{targetId}")]
        [Authorize]
        public async Task<IActionResult> Unfollow([FromRoute] string targetId)
        {
            await _profileService.UnfollowAsync(UserId!, targetId);
            return NoContent();
        }

        [HttpGet("blocked")]
        [Authorize]
        public async Task<IActionResult> GetBlocked()
        {
            return Ok(await _profileService.GetBlockedAsync(UserId!));
        }

        [HttpPost("block/{targetId}")]
        [Authorize]
        public async Task<IActionResult> Block([FromRoute] string targetId)
        {
            await _profileService.BlockAsync(UserId!, targetId);
            return NoContent();
        }

        [HttpDelete("block/{targetId}")]
        [Authorize]
        public async Task<IActionResult> Unblock([FromRoute] string targetId)
        {
            await _profileService.UnblockAsync(UserId!, targetId);
            return NoContent();
        }
    }
}
