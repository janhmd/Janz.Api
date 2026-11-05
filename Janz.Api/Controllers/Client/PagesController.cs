using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    // Mətn səhifələri (Delivery, Refunds, FAQ və s.) hamıya açıqdır
    [Route("api/[controller]")]
    [ApiController]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _pageService;

        public PagesController(IPageService pageService)
        {
            _pageService = pageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLinks()
        {
            return Ok(await _pageService.GetLinksAsync());
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug([FromRoute] string slug)
        {
            return Ok(await _pageService.GetBySlugAsync(slug));
        }
    }
}
