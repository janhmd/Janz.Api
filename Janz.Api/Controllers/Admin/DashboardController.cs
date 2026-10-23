using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Interfaces;

namespace Janz.Api.Controllers.Admin
{
    // Admin panelin ana səhifəsi: satışlar, sifarişlər, müştərilər və az qalan stok. Bütün işçilər görür
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize(Roles = Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.Editor + "," + Roles.Support)]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _dashboardService.GetAsync());
        }
    }
}
