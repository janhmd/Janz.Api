using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Orders;
using Service.Helpers.DTOs.Users;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    // Admin panel: istifadəçilər. Bloklama Identity-nin lockout-u ilədir (LockoutEnd = sonsuz):
    // bloklanmış istifadəçi daxil ola bilmir, köhnə tokeni də işləmir (Program.cs -> OnTokenValidated)
    public class UserAdminService : IUserAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOrderRepository _orderRepo;
        private readonly IValidator<UpdateUserRoleDto> _roleValidator;

        public UserAdminService(UserManager<AppUser> userManager,
                                IOrderRepository orderRepo,
                                IValidator<UpdateUserRoleDto> roleValidator)
        {
            _userManager = userManager;
            _orderRepo = orderRepo;
            _roleValidator = roleValidator;
        }

        public async Task<IEnumerable<UserAdminDto>> GetAllAsync(string? role, bool? blocked, string? search)
        {
            if (!string.IsNullOrWhiteSpace(role) && !Roles.All.Contains(role))
            {
                throw new BadRequestException("This role is not known.");
            }

            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var roles = await GetRoleMapAsync();
            var orders = (await _orderRepo.GetAllForAdminAsync(null)).ToLookup(m => m.UserId);

            var result = users.Select(m => ToDto(m, roles.GetValueOrDefault(m.Id, Roles.User), orders[m.Id]));

            if (!string.IsNullOrWhiteSpace(role))
            {
                result = result.Where(m => m.Role == role);
            }

            if (blocked is not null)
            {
                result = result.Where(m => m.IsBlocked == blocked);
            }

            // Axtarış: ad, soyad və ya email
            if (!string.IsNullOrWhiteSpace(search))
            {
                var text = search.Trim();
                result = result.Where(m =>
                    m.FullName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                    m.Email.Contains(text, StringComparison.OrdinalIgnoreCase));
            }

            return result.OrderByDescending(m => m.CreatedAt).ToList();
        }

        public async Task<UserAdminDetailDto> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                throw new NotFoundException();
            }

            var role = Roles.Highest(await _userManager.GetRolesAsync(user));
            var orders = await _orderRepo.GetByUserAsync(id);

            var dto = new UserAdminDetailDto();
            Fill(dto, user, role, orders);
            dto.Orders = orders.Select(m => new OrderDto
            {
                Id = m.Id,
                Status = m.Status.ToString(),
                Total = m.Total,
                CreatedAt = m.CreatedAt,
                DeliveryFullName = m.DeliveryFullName,
                DeliveryPhone = m.DeliveryPhone,
                DeliveryLine = m.DeliveryLine,
                DeliveryCity = m.DeliveryCity,
                DeliveryPostalCode = m.DeliveryPostalCode
            }).ToList();
            return dto;
        }

        public async Task BlockAsync(string id, string staffId, bool staffIsSuperAdmin)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                throw new NotFoundException();
            }

            if (user.Id == staffId)
            {
                throw new BadRequestException("You can't block yourself.");
            }

            var role = Roles.Highest(await _userManager.GetRolesAsync(user));

            // SuperAdmin heç vaxt bloklanmır; digər işçiləri (Admin, Editor, Support) yalnız SuperAdmin bloklaya bilər
            if (role == Roles.SuperAdmin)
            {
                throw new BadRequestException("A SuperAdmin can't be blocked.");
            }

            if (role != Roles.User && !staffIsSuperAdmin)
            {
                throw new BadRequestException("Only a SuperAdmin can block staff.");
            }

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }

        public async Task UnblockAsync(string id, bool staffIsSuperAdmin)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                throw new NotFoundException();
            }

            var role = Roles.Highest(await _userManager.GetRolesAsync(user));
            if (role != Roles.User && !staffIsSuperAdmin)
            {
                throw new BadRequestException("Only a SuperAdmin can unblock staff.");
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync()
        {
            var users = await _userManager.Users.AsNoTracking().Select(m => m.Id).ToListAsync();
            var roles = await GetRoleMapAsync();

            // Rolu olmayan köhnə hesablar da "User" sayılır
            var counts = users
                .GroupBy(id => roles.GetValueOrDefault(id, Roles.User))
                .ToDictionary(m => m.Key, m => m.Count());

            return Roles.All.AsEnumerable().Reverse().Select(m => new RoleDto
            {
                Name = m,
                Description = Roles.Descriptions[m],
                UserCount = counts.GetValueOrDefault(m)
            }).ToList();
        }

        public async Task ChangeRoleAsync(string id, UpdateUserRoleDto model, string staffId)
        {
            await _roleValidator.EnsureValidAsync(model);
            var role = model.Role!;

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                throw new NotFoundException();
            }

            // SuperAdmin öz rolunu dəyişə bilməz: təsadüfən panelə girişi itirməsin
            if (user.Id == staffId)
            {
                throw new BadRequestException("You can't change your own role.");
            }

            var current = await _userManager.GetRolesAsync(user);
            if (current.Count == 1 && current[0] == role)
            {
                return;
            }

            // Son SuperAdmin rolunu itirə bilməz: sistemdə rol verə bilən heç kim qalmazdı
            if (current.Contains(Roles.SuperAdmin) && role != Roles.SuperAdmin)
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync(Roles.SuperAdmin);
                if (superAdmins.Count <= 1)
                {
                    throw new BadRequestException("This is the last SuperAdmin, so the role can't be taken away.");
                }
            }

            // Hər istifadəçinin bir rolu olur: köhnələr silinir, yenisi verilir
            if (current.Count > 0)
            {
                var removed = await _userManager.RemoveFromRolesAsync(user, current);
                if (!removed.Succeeded)
                {
                    throw new BadRequestException(removed.Errors.Select(e => e.Description).ToArray());
                }
            }

            var added = await _userManager.AddToRoleAsync(user, role);
            if (!added.Succeeded)
            {
                throw new BadRequestException(added.Errors.Select(e => e.Description).ToArray());
            }
        }

        // Hər istifadəçinin ən yüksək rolu: hər rol üçün bir sorğu (istifadəçi başına sorğu yox)
        private async Task<Dictionary<string, string>> GetRoleMapAsync()
        {
            var map = new Dictionary<string, string>();
            foreach (var role in Roles.All)
            {
                foreach (var user in await _userManager.GetUsersInRoleAsync(role))
                {
                    map[user.Id] = map.TryGetValue(user.Id, out var current)
                        ? Roles.Highest(new[] { current, role })
                        : role;
                }
            }

            return map;
        }

        private static UserAdminDto ToDto(AppUser user, string role, IEnumerable<Order> orders)
        {
            var dto = new UserAdminDto();
            Fill(dto, user, role, orders);
            return dto;
        }

        private static void Fill(UserAdminDto dto, AppUser user, string role, IEnumerable<Order> orders)
        {
            var list = orders.ToList();
            dto.Id = user.Id;
            dto.FullName = (user.Name + " " + user.Surname).Trim();
            dto.Email = user.Email ?? string.Empty;
            dto.Role = role;
            dto.IsBlocked = user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow;
            dto.CreatedAt = user.CreatedAt;
            dto.OrderCount = list.Count;
            dto.TotalSpent = list.Where(m => m.Status != OrderStatus.Cancelled).Sum(m => m.Total);
        }
    }
}
