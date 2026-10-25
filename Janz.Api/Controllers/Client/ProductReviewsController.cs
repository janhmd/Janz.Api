using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Reviews;
using Service.Services.Interfaces;
using System.Security.Claims;

namespace Janz.Api.Controllers.Client
{
    // Məhsulun rəyləri: oxumaq hamıya açıqdır, yazmaq yalnız məhsulu alan istifadəçiyə
    [Route("api/products/{productId:int}/reviews")]
    [ApiController]
    public class ProductReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ProductReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> Get([FromRoute] int productId)
        {
            return Ok(await _reviewService.GetForProductAsync(productId, UserId));
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Save([FromRoute] int productId, [FromBody] SaveProductReviewDto model)
        {
            return Ok(await _reviewService.SaveForProductAsync(productId, UserId!, model));
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int productId)
        {
            await _reviewService.DeleteForProductAsync(productId, UserId!);
            return NoContent();
        }
    }
}
