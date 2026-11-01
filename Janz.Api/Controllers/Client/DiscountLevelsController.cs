using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    // Müştərinin endirim səviyyəsi (səbət və hesab)
    [Route("api/discount-levels")]
    [ApiController]
    [Authorize]
    public class DiscountLevelsController : ControllerBase
    {
        private readonly IDiscountLevelService _levelService;

        public DiscountLevelsController(IDiscountLevelService levelService)
        {
            _levelService = levelService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            return Ok(await _levelService.GetForUserAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty));
        }
    }
}
