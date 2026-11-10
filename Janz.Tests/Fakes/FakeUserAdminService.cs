using Service.Helpers.DTOs.Users;
using Service.Services.Interfaces;

namespace Janz.Tests.Fakes
{
    // Yalnız siyahını qaytarır (rol və bloklanma süzgəci ilə): kupon göndərişi testləri üçün
    public class FakeUserAdminService : IUserAdminService
    {
        public List<UserAdminDto> Users { get; } = new();

        public Task<IEnumerable<UserAdminDto>> GetAllAsync(string? role, bool? blocked, string? search)
        {
            var result = Users.Where(m => (role is null || m.Role == role) && (blocked is null || m.IsBlocked == blocked));
            return Task.FromResult<IEnumerable<UserAdminDto>>(result.ToList());
        }

        public Task<UserAdminDetailDto> GetByIdAsync(string id) => throw new NotImplementedException();
        public Task BlockAsync(string id, string staffId, bool staffIsSuperAdmin) => throw new NotImplementedException();
        public Task UnblockAsync(string id, bool staffIsSuperAdmin) => throw new NotImplementedException();
        public Task<IEnumerable<RoleDto>> GetRolesAsync() => throw new NotImplementedException();
        public Task ChangeRoleAsync(string id, UpdateUserRoleDto model, string staffId) => throw new NotImplementedException();
    }
}
