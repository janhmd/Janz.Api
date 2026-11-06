using Service.Helpers.DTOs.Dashboard;
using Service.Services.Interfaces;

namespace Janz.Tests.Fakes
{
    // Bazaya getmədən hazır dashboard rəqəmlərini qaytarır
    public class FakeDashboardService : IDashboardService
    {
        public DashboardDto Dashboard { get; set; } = new DashboardDto();

        public Task<DashboardDto> GetAsync() => Task.FromResult(Dashboard);
    }
}
