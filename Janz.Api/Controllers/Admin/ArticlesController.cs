using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Articles;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: jurnal məqalələri. Redaktor, Admin və SuperAdmin
    [Route("api/admin/articles")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Editor)]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _articleService.GetAllAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            return Ok(await _articleService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateArticleDto request)
        {
            return Ok(await _articleService.CreateAsync(request));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromForm] UpdateArticleDto request)
        {
            await _articleService.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _articleService.DeleteAsync(id);
            return NoContent();
        }
    }
}
