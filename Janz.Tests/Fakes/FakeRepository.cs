using Domain.Common;
using Repository.Repositories.Interfaces;

namespace Janz.Tests.Fakes
{
    // Bazaya getmədən yaddaşda işləyən repository: servis testləri üçün
    public class FakeRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        public List<T> Rows { get; } = new List<T>();

        public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult<IEnumerable<T>>(Rows.ToList());

        public Task<T?> GetAsync() => Task.FromResult(Rows.FirstOrDefault());

        public Task<T?> GetByIdAsync(int id) => Task.FromResult(Rows.FirstOrDefault(m => m.Id == id));

        public Task AddAsync(T entity)
        {
            entity.Id = Rows.Count == 0 ? 1 : Rows.Max(m => m.Id) + 1;
            Rows.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity) => Task.CompletedTask;

        public Task DeleteAsync(T entity)
        {
            Rows.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
