using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Assistant;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    // Sayt köməkçisi: hamıya açıqdır, cavabı Gemini verir (açar yoxdursa Answer boşdur)
    [Route("api/[controller]")]
    [ApiController]
    public class AssistantController : ControllerBase
    {
        private readonly IAssistantService _assistantService;

        public AssistantController(IAssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskAssistantDto request)
        {
            return Ok(await _assistantService.AskAsync(request));
        }
    }
}
