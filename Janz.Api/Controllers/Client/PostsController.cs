using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Community;
using Service.Services.Interfaces;
using System.Security.Claims;

namespace Janz.Api.Controllers.Client
{
    // Community lenti: oxumaq hamıya açıqdır; yazmaq, səs vermək, saxlamaq və şikayət üçün giriş lazımdır
    [Route("api/posts")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IPostCommentService _commentService;

        public PostsController(IPostService postService, IPostCommentService commentService)
        {
            _postService = postService;
            _commentService = commentService;
        }

        private string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetFeed([FromQuery] string? tab, [FromQuery] string? tag, [FromQuery] string? search, [FromQuery] string? scope)
        {
            return Ok(await _postService.GetFeedAsync(tab, tag, search, scope, UserId));
        }

        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProduct([FromRoute] int productId)
        {
            return Ok(await _postService.GetByProductAsync(productId, UserId));
        }

        [HttpGet("saved")]
        [Authorize]
        public async Task<IActionResult> GetSaved()
        {
            return Ok(await _postService.GetSavedAsync(UserId!));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreatePostDto request)
        {
            return Ok(await _postService.CreateAsync(UserId!, request));
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            await _postService.DeleteAsync(UserId!, id);
            return NoContent();
        }

        [HttpPost("{id:int}/vote")]
        [Authorize]
        public async Task<IActionResult> Vote([FromRoute] int id, [FromBody] PostVoteDto request)
        {
            return Ok(await _postService.VoteAsync(UserId!, id, request));
        }

        [HttpPost("{id:int}/save")]
        [Authorize]
        public async Task<IActionResult> Save([FromRoute] int id)
        {
            await _postService.SaveAsync(UserId!, id, true);
            return NoContent();
        }

        [HttpDelete("{id:int}/save")]
        [Authorize]
        public async Task<IActionResult> Unsave([FromRoute] int id)
        {
            await _postService.SaveAsync(UserId!, id, false);
            return NoContent();
        }

        [HttpPost("{id:int}/report")]
        [Authorize]
        public async Task<IActionResult> Report([FromRoute] int id, [FromBody] ReportPostDto request)
        {
            await _postService.ReportAsync(UserId!, id, request);
            return NoContent();
        }

        [HttpPost("{id:int}/comments")]
        [Authorize]
        public async Task<IActionResult> AddComment([FromRoute] int id, [FromForm] CreatePostCommentDto request)
        {
            return Ok(await _commentService.AddAsync(UserId!, id, request));
        }

        [HttpDelete("comments/{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment([FromRoute] int id)
        {
            await _commentService.DeleteAsync(UserId!, id);
            return NoContent();
        }

        [HttpPost("comments/{id:int}/vote")]
        [Authorize]
        public async Task<IActionResult> VoteComment([FromRoute] int id, [FromBody] PostVoteDto request)
        {
            return Ok(await _commentService.VoteAsync(UserId!, id, request));
        }

        [HttpPost("comments/{id:int}/report")]
        [Authorize]
        public async Task<IActionResult> ReportComment([FromRoute] int id, [FromBody] ReportPostDto request)
        {
            await _commentService.ReportAsync(UserId!, id, request);
            return NoContent();
        }
    }
}
