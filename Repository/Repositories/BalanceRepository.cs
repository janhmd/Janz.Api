using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories
{
    public class BalanceRepository : BaseRepository<BalanceTransaction>, IBalanceRepository
    {
        public BalanceRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<decimal> GetBalanceAsync(string userId)
        {
            return await _dbSet
                    .Where(m => m.UserId == userId && m.IsConfirmed)
                    .SumAsync(m => (decimal?)m.Amount) ?? 0;
        }

        public async Task<IReadOnlyList<BalanceTransaction>> GetByUserAsync(string userId)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Where(m => m.UserId == userId && m.IsConfirmed)
                    .OrderByDescending(m => m.CreatedAt)
                    .ThenByDescending(m => m.Id)
                    .ToListAsync();
        }

        public async Task<BalanceTransaction?> GetBySessionAsync(string userId, string sessionId)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.UserId == userId && m.PaymentSessionId == sessionId);
        }

        public async Task<BalanceTransaction?> RedeemCertificateAsync(string userId, string code)
        {
            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var taken = await _dbContext.GiftCertificates
                    .Where(m => m.Code == code && m.IsActive && m.UsedById == null && (m.ExpiresAt == null || m.ExpiresAt > now))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(m => m.UsedById, userId)
                        .SetProperty(m => m.UsedAt, now));
            if (taken == 0)
            {
                return null;
            }

            var certificate = await _dbContext.GiftCertificates.AsNoTracking().FirstAsync(m => m.Code == code);
            var entry = new BalanceTransaction
            {
                UserId = userId,
                Amount = certificate.Amount,
                Type = BalanceTransactionType.Certificate,
                Note = $"Certificate {certificate.Code}",
                IsConfirmed = true
            };
            await _dbSet.AddAsync(entry);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
            return entry;
        }
    }
}
