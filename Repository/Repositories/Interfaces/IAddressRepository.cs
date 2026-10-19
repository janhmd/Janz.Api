using Domain.Entities;

namespace Repository.Repositories.Interfaces
{
    public interface IAddressRepository : IBaseRepository<Address>
    {
        // Yalnız həmin istifadəçinin ünvanları: GetAllAsync() hamının ünvanını qaytarır, istifadəçiyə açılmamalıdır
        Task<IReadOnlyList<Address>> GetByUserAsync(string userId);

        // Ünvan yalnız sahibinə aiddirsə tapılır (başqasının ünvanına baxmağın qarşısını alır). Dəyişdirmək üçün izlənir.
        Task<Address?> GetByIdForUserAsync(int id, string userId);
    }
}
