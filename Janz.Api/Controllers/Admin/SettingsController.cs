using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Helpers.DTOs.Settings;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panel: sayt parametrləri. Yalnız SuperAdmin
    [Route("api/admin/settings")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingService _settingService;

        public SettingsController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _settingService.GetAsync());
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSiteSettingDto request)
        {
            await _settingService.UpdateAsync(request);
            return NoContent();
        }
    }
}
