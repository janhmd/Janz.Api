using Service.Helpers.DTOs.Dashboard;

namespace Service.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetAsync();
    }
}
