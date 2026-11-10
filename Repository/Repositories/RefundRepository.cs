using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories
{
    public class RefundRepository : BaseRepository<RefundRequest>, IRefundRepository
    {
        public RefundRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<RefundRequest?> GetLatestByOrderAsync(int orderId)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Where(m => m.OrderId == orderId)
                    .OrderByDescending(m => m.CreatedAt)
                    .ThenByDescending(m => m.Id)
                    .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<RefundRequest>> GetAllForAdminAsync(RefundStatus? status)
        {
            var query = _dbSet
                    .AsNoTracking()
                    .Include(m => m.Order).ThenInclude(o => o.User)
                    .AsQueryable();

            if (status is not null)
            {
                query = query.Where(m => m.Status == status);
            }

            return await query
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();
        }

        public async Task<RefundRequest?> GetByIdForAdminAsync(int id)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Include(m => m.Order).ThenInclude(o => o.User)
                    .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<bool> ApproveAsync(int id, decimal amount, string? note)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            // Şərt bazada yoxlanılır: sorğu artıq həll olunubsa heç nə dəyişmir
            var changed = await _dbSet
                    .Where(m => m.Id == id && m.Status == RefundStatus.Pending)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.Status, RefundStatus.Approved)
                        .SetProperty(m => m.Amount, amount)
                        .SetProperty(m => m.StaffNote, note)
                        .SetProperty(m => m.ResolvedAt, DateTime.UtcNow));

            if (changed == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            var request = await _dbSet.AsNoTracking().FirstAsync(m => m.Id == id);
            _dbContext.BalanceTransactions.Add(new BalanceTransaction
            {
                UserId = request.UserId,
                Amount = amount,
                Type = BalanceTransactionType.Refund,
                Note = $"Refund for order #{request.OrderId}",
                OrderId = request.OrderId,
                IsConfirmed = true
            });
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int id, string? note)
        {
            var changed = await _dbSet
                    .Where(m => m.Id == id && m.Status == RefundStatus.Pending)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.Status, RefundStatus.Rejected)
                        .SetProperty(m => m.StaffNote, note)
                        .SetProperty(m => m.ResolvedAt, DateTime.UtcNow));

            return changed > 0;
        }
    }
}
