using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panelin bildiriş mərkəzi: hər işçi yalnız rolunun görə biləcəyi bildirişləri alır
    [Route("api/admin/notifications")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Editor + "," + Roles.Support)]
    public class NotificationsController : ControllerBase
    {
        private readonly IAdminNotificationService _notificationService;

        public NotificationsController(IAdminNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var manager = User.IsInRole(Roles.SuperAdmin) || User.IsInRole(Roles.Admin);
            var types = new List<string>();
            if (manager || User.IsInRole(Roles.Support)) types.Add("order");
            if (manager || User.IsInRole(Roles.Editor)) { types.Add("report"); types.Add("review"); }
            if (manager) types.Add("stock");

            return Ok(await _notificationService.GetAsync(types));
        }
    }
}
