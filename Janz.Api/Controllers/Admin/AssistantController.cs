using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Assistant;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin köməkçisi: bütün işçilər soruşa bilər, cavabı Gemini verir (açar yoxdursa Answer boşdur)
    [Route("api/admin/assistant")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Editor + "," + Roles.Support)]
    public class AssistantController : ControllerBase
    {
        private readonly IAdminAssistantService _adminAssistantService;

        public AssistantController(IAdminAssistantService adminAssistantService)
        {
            _adminAssistantService = adminAssistantService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskAssistantDto request)
        {
            return Ok(await _adminAssistantService.AskAsync(request));
        }
    }
}
