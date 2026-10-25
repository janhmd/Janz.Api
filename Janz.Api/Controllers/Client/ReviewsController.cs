using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Reviews;
using Service.Services.Interfaces;
using System.Security.Claims;

namespace Janz.Api.Controllers.Client
{
    // Mağaza rəyləri: siyahı hamıya açıqdır, rəyi yalnız sifarişin sahibi yazır (sifariş çatdırılandan sonra)
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        // ?skip=0&take=20
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            return Ok(await _reviewService.GetShopReviewsAsync(skip, take));
        }

        [HttpGet("order/{orderId:int}")]
        [Authorize]
        public async Task<IActionResult> GetForOrder([FromRoute] int orderId)
        {
            return Ok(await _reviewService.GetForOrderAsync(orderId, UserId));
        }

        [HttpPut("order/{orderId:int}")]
        [Authorize]
        public async Task<IActionResult> SaveForOrder([FromRoute] int orderId, [FromBody] SaveShopReviewDto model)
        {
            return Ok(await _reviewService.SaveForOrderAsync(orderId, UserId, model));
        }
    }
}
