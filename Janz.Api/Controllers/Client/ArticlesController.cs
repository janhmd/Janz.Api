using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Articles;
using Service.Services.Interfaces;
using System.Security.Claims;

namespace Janz.Api.Controllers.Client
{
    // Jurnal: oxumaq hamıya açıqdır; bəyənmək, saxlamaq və şərh yazmaq üçün giriş lazımdır
    [Route("api/articles")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? topic, [FromQuery] string? tag, [FromQuery] string? sort)
        {
            return Ok(await _articleService.GetPublishedAsync(topic, tag, sort, UserId));
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug([FromRoute] string slug, [FromQuery] bool view = false)
        {
            return Ok(await _articleService.GetBySlugAsync(slug, UserId, view));
        }

        [HttpGet("saved")]
        [Authorize]
        public async Task<IActionResult> GetSaved()
        {
            return Ok(await _articleService.GetSavedAsync(UserId!));
        }

        [HttpPost("{id:int}/like")]
        [Authorize]
        public async Task<IActionResult> Like([FromRoute] int id)
        {
            return Ok(new { likes = await _articleService.LikeAsync(id, UserId!, true) });
        }

        [HttpDelete("{id:int}/like")]
        [Authorize]
        public async Task<IActionResult> Unlike([FromRoute] int id)
        {
            return Ok(new { likes = await _articleService.LikeAsync(id, UserId!, false) });
        }

        [HttpPost("{id:int}/save")]
        [Authorize]
        public async Task<IActionResult> Save([FromRoute] int id)
        {
            await _articleService.SaveAsync(id, UserId!, true);
            return NoContent();
        }

        [HttpDelete("{id:int}/save")]
        [Authorize]
        public async Task<IActionResult> Unsave([FromRoute] int id)
        {
            await _articleService.SaveAsync(id, UserId!, false);
            return NoContent();
        }

        [HttpGet("{id:int}/comments")]
        public async Task<IActionResult> GetComments([FromRoute] int id)
        {
            return Ok(await _articleService.GetCommentsAsync(id, UserId));
        }

        [HttpPost("{id:int}/comments")]
        [Authorize]
        public async Task<IActionResult> AddComment([FromRoute] int id, [FromBody] CreateArticleCommentDto model)
        {
            return Ok(await _articleService.AddCommentAsync(id, UserId!, model));
        }

        [HttpDelete("comments/{commentId:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment([FromRoute] int commentId)
        {
            var isStaff = User.IsInRole(Roles.Editor) || User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SuperAdmin);
            await _articleService.DeleteCommentAsync(commentId, UserId!, isStaff);
            return NoContent();
        }

        [HttpPost("comments/{commentId:int}/vote")]
        [Authorize]
        public async Task<IActionResult> Vote([FromRoute] int commentId, [FromBody] ArticleVoteDto model)
        {
            return Ok(await _articleService.VoteAsync(commentId, UserId!, model));
        }
    }
}
