using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories
{
    public class AddressRepository : BaseRepository<Address>, IAddressRepository
    {
        public AddressRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Address>> GetByUserAsync(string userId)
        {
            return await _dbSet
                    .AsNoTracking()
                    .Where(m => m.UserId == userId)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();
        }

        public async Task<Address?> GetByIdForUserAsync(int id, string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        }
    }
}
