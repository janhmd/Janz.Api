using Service.Helpers.DTOs.Orders;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllAsync(string userId);
        Task<ServiceResult<OrderDto>> GetByIdAsync(string userId, int id);
        Task<ServiceResult<OrderDto>> CreateFromCartAsync(string userId, CreateOrderDto model);

        // Admin panel: statusa və müştəriyə görə axtarış, tək sifariş, statusu dəyişmək
        Task<IEnumerable<AdminOrderDto>> GetAllForAdminAsync(string? status, string? search);
        Task<AdminOrderDto> GetByIdForAdminAsync(int id);
        Task ChangeStatusAsync(int id, UpdateOrderStatusDto model);
    }
}
