using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Community;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class CommunityAuthorService : ICommunityAuthorService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IBaseRepository<DiscountLevel> _levelRepo;
        private readonly IBaseRepository<Order> _orderRepo;
        private readonly IBaseRepository<Address> _addressRepo;

        public CommunityAuthorService(UserManager<AppUser> userManager,
                                      IBaseRepository<DiscountLevel> levelRepo,
                                      IBaseRepository<Order> orderRepo,
                                      IBaseRepository<Address> addressRepo)
        {
            _userManager = userManager;
            _levelRepo = levelRepo;
            _orderRepo = orderRepo;
            _addressRepo = addressRepo;
        }

        public async Task<Dictionary<string, AuthorDto>> GetAsync(IEnumerable<string> userIds)
        {
            var ids = userIds.Distinct().ToList();
            var levels = (await _levelRepo.GetAllAsync()).OrderBy(m => m.MinSpent).ToList();
            var spent = (await _orderRepo.GetAllAsync())
                .Where(m => ids.Contains(m.UserId) && m.Status == OrderStatus.Delivered)
                .GroupBy(m => m.UserId)
                .ToDictionary(m => m.Key, m => m.Sum(o => o.Total));
            var cities = (await _addressRepo.GetAllAsync())
                .Where(m => ids.Contains(m.UserId))
                .GroupBy(m => m.UserId)
                .ToDictionary(m => m.Key, m => m.OrderBy(a => a.Id).First().City);

            var result = new Dictionary<string, AuthorDto>();
            foreach (var id in ids)
            {
                var user = await _userManager.FindByIdAsync(id);
                var level = levels.LastOrDefault(m => m.MinSpent <= spent.GetValueOrDefault(id))?.Name ?? "Coffee lover";
                var city = cities.GetValueOrDefault(id);

                result[id] = new AuthorDto
                {
                    UserId = id,
                    Name = user is null ? "Coffee lover" : DisplayName(user),
                    Nickname = user?.Nickname,
                    AvatarPath = user?.AvatarPath,
                    Role = string.IsNullOrWhiteSpace(city) ? level : $"{level}, {city}",
                    StaffRole = user is null ? null : StaffRoleOf(await _userManager.GetRolesAsync(user))
                };
            }

            return result;
        }

        public static string? StaffRoleOf(IEnumerable<string> roles)
        {
            var role = Domain.Constants.Roles.Highest(roles);
            return Domain.Constants.Roles.Staff.Contains(role) ? role : null;
        }

        private static string DisplayName(AppUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.Nickname))
            {
                return user.Nickname;
            }

            var name = user.Name + (string.IsNullOrWhiteSpace(user.Surname) ? string.Empty : $" {user.Surname[0]}.");
            return string.IsNullOrWhiteSpace(name) ? "Coffee lover" : name.Trim();
        }
    }
}
