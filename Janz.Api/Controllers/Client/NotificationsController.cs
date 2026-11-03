using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    // Bildirişlər: header-dəki zəng və hesabdakı Notifications bölməsi
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string UserId
        {
            get
            {
                return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _notificationService.GetAllAsync(UserId));
        }

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkRead([FromRoute] int id)
        {
            await _notificationService.MarkReadAsync(UserId, id);
            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllReadAsync(UserId);
            return NoContent();
        }
    }
}
