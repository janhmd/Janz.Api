using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.DiscountLevels;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: endirim səviyyələri. Yalnız Admin və SuperAdmin dəyişə bilər
    [Route("api/admin/discount-levels")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin)]
    public class DiscountLevelsController : ControllerBase
    {
        private readonly IDiscountLevelService _levelService;

        public DiscountLevelsController(IDiscountLevelService levelService)
        {
            _levelService = levelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _levelService.GetAllAsync());
        }

        // Bütün cədvəl birdən saxlanılır
        [HttpPut]
        public async Task<IActionResult> Save([FromBody] SaveDiscountLevelsDto request)
        {
            await _levelService.SaveAsync(request);
            return NoContent();
        }
    }
}
