using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Pages;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: mətn səhifələri
    [Route("api/admin/pages")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Editor)]
    public class PagesController : ControllerBase
    {
        private readonly IPageService _pageService;

        public PagesController(IPageService pageService)
        {
            _pageService = pageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _pageService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            return Ok(await _pageService.GetByIdAsync(id));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSitePageDto request)
        {
            await _pageService.UpdateAsync(id, request);
            return NoContent();
        }
    }
}
