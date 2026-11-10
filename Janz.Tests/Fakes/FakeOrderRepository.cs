using Domain.Entities;
using Domain.Enums;
using Repository.Repositories.Interfaces;
using Repository.Results;

namespace Janz.Tests.Fakes
{
    // Bazaya getmədən yaddaşda sifarişlər: yalnız admin siyahısı işləyir, qalanı bu testlərdə lazım deyil
    public class FakeOrderRepository : FakeRepository<Order>, IOrderRepository
    {
        public Task<IReadOnlyList<Order>> GetAllForAdminAsync(OrderStatus? status)
        {
            IReadOnlyList<Order> rows = Rows.Where(m => status is null || m.Status == status).ToList();
            return Task.FromResult(rows);
        }

        public Task<IReadOnlyList<Order>> GetByUserAsync(string userId) => throw new NotSupportedException();

        public Task<Order?> GetByIdForUserAsync(int id, string userId) => throw new NotSupportedException();

        public Task<OrderPlaceResult> PlaceAsync(OrderPlan plan) => throw new NotSupportedException();

        public Task<Order?> GetByIdForAdminAsync(int id) => throw new NotSupportedException();

        public Task<bool> ChangeStatusAsync(int id, OrderStatus from, OrderStatus to) => throw new NotSupportedException();

        public Task<Order?> GetAnyByIdAsync(int id) => throw new NotSupportedException();

        public Task<IReadOnlyList<Order>> GetAwaitingAsync(string? userId, DateTime? createdBefore) => throw new NotSupportedException();

        public Task<bool> FinalizePaidAsync(int id) => throw new NotSupportedException();

        public Task<bool> DiscardAsync(int id) => throw new NotSupportedException();
    }
}
