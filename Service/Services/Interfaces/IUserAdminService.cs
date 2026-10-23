using Service.Helpers.DTOs.Users;

namespace Service.Services.Interfaces
{
    public interface IUserAdminService
    {
        // ?role=Admin&blocked=true&search=aysel
        Task<IEnumerable<UserAdminDto>> GetAllAsync(string? role, bool? blocked, string? search);
        Task<UserAdminDetailDto> GetByIdAsync(string id);

        // staffId: əməliyyatı edən işçi (özünü bloklaya bilməz), staffIsSuperAdmin: işçini yalnız SuperAdmin bloklayır
        Task BlockAsync(string id, string staffId, bool staffIsSuperAdmin);
        Task UnblockAsync(string id, bool staffIsSuperAdmin);

        // Yalnız SuperAdmin: rollar və onlarda neçə istifadəçi olduğu, istifadəçiyə rol vermək
        Task<IEnumerable<RoleDto>> GetRolesAsync();
        Task ChangeRoleAsync(string id, UpdateUserRoleDto model, string staffId);
    }
}
