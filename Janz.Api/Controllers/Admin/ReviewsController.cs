using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: məhsul və mağaza rəyləri (Editor da idarə edir). Gizlədilən rəy saytda görünmür və reytinqə daxil olmur
    [Route("api/admin/reviews")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Editor)]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // ?type=product|shop&hidden=true&search=aysel
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type, [FromQuery] bool? hidden, [FromQuery] string? search)
        {
            return Ok(await _reviewService.GetAllForAdminAsync(type, hidden, search));
        }

        [HttpPut("{type}/{id:int}/hide")]
        public async Task<IActionResult> Hide([FromRoute] string type, [FromRoute] int id)
        {
            await _reviewService.SetHiddenAsync(type, id, true);
            return NoContent();
        }

        [HttpPut("{type}/{id:int}/show")]
        public async Task<IActionResult> Show([FromRoute] string type, [FromRoute] int id)
        {
            await _reviewService.SetHiddenAsync(type, id, false);
            return NoContent();
        }

        [HttpDelete("{type}/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] string type, [FromRoute] int id)
        {
            await _reviewService.DeleteAsync(type, id);
            return NoContent();
        }
    }
}
