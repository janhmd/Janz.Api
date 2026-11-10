using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Community;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: Community moderasiyası. Redaktor, Admin və SuperAdmin
    [Route("api/admin/community")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
    public class CommunityController : ControllerBase
    {
        private readonly ICommunityAdminService _adminService;

        public CommunityController(ICommunityAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("posts")]
        public async Task<IActionResult> GetPosts()
        {
            return Ok(await _adminService.GetPostsAsync());
        }

        [HttpGet("comments")]
        public async Task<IActionResult> GetComments()
        {
            return Ok(await _adminService.GetCommentsAsync());
        }

        [HttpGet("reports")]
        public async Task<IActionResult> GetReports()
        {
            return Ok(await _adminService.GetReportsAsync());
        }

        [HttpPut("posts/{id:int}/hidden")]
        public async Task<IActionResult> SetPostHidden([FromRoute] int id, [FromBody] SetHiddenDto request)
        {
            await _adminService.SetPostHiddenAsync(id, request);
            return NoContent();
        }

        [HttpPut("comments/{id:int}/hidden")]
        public async Task<IActionResult> SetCommentHidden([FromRoute] int id, [FromBody] SetHiddenDto request)
        {
            await _adminService.SetCommentHiddenAsync(id, request);
            return NoContent();
        }

        [HttpDelete("posts/{id:int}")]
        public async Task<IActionResult> DeletePost([FromRoute] int id)
        {
            await _adminService.DeletePostAsync(id);
            return NoContent();
        }

        [HttpDelete("comments/{id:int}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int id)
        {
            await _adminService.DeleteCommentAsync(id);
            return NoContent();
        }

        [HttpPut("reports/{id:int}/resolve")]
        public async Task<IActionResult> ResolveReport([FromRoute] int id)
        {
            await _adminService.ResolveReportAsync(id);
            return NoContent();
        }
    }
}
