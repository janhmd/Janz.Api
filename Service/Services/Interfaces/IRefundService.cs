using Service.Helpers.DTOs.Refunds;
using Service.Helpers.Responses;

namespace Service.Services.Interfaces
{
    public interface IRefundService
    {
        // Müştəri: çatdırılmış sifariş üçün pulu geri istəyir
        Task<ServiceResult<RefundDto>> CreateAsync(string userId, int orderId, CreateRefundDto model);

        // Admin panel: sorğular, tək sorğu, qəbul (balansa qayıdır) və rədd
        Task<IEnumerable<AdminRefundDto>> GetAllForAdminAsync(string? status);
        Task<AdminRefundDto> GetByIdForAdminAsync(int id);
        Task ApproveAsync(int id, ResolveRefundDto model);
        Task RejectAsync(int id, ResolveRefundDto model);
    }
}
