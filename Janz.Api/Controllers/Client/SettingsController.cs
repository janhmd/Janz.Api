using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Client
{
    // Saytın əlaqə məlumatları (footer, Contacts) hamıya açıqdır
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
