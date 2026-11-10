using Domain.Entities;
using Domain.Enums;

namespace Repository.Repositories.Interfaces
{
    public interface IRefundRepository : IBaseRepository<RefundRequest>
    {
        // Sifarişin ən son sorğusu (yoxdursa null)
        Task<RefundRequest?> GetLatestByOrderAsync(int orderId);

        // Admin: sorğular (sifarişi və müştərisi ilə), istəsən yalnız bir statusda olanlar
        Task<IReadOnlyList<RefundRequest>> GetAllForAdminAsync(RefundStatus? status);

        Task<RefundRequest?> GetByIdForAdminAsync(int id);

        // Qəbul: sorğu hələ Pending-dirsə "Approved" olur və məbləğ eyni tranzaksiyada müştərinin balansına yazılır.
        // İki işçi eyni anda qəbul etsə yalnız biri keçir (ikinci false alır, pul iki dəfə qayıtmır)
        Task<bool> ApproveAsync(int id, decimal amount, string? note);

        // Rədd: sorğu hələ Pending-dirsə "Rejected" olur
        Task<bool> RejectAsync(int id, string? note);
    }
}
