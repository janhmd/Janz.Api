using Service.Helpers.DTOs.Orders;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllAsync(string userId);
        Task<ServiceResult<OrderDto>> GetByIdAsync(string userId, int id);
        Task<ServiceResult<OrderDto>> CreateFromCartAsync(string userId);
    }
}
