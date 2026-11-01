using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.DiscountLevels;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    // Səviyyə müştərinin çatdırılmış sifarişlərinə xərclədiyi məbləğə görədir
    public class DiscountLevelService : IDiscountLevelService
    {
        private readonly IBaseRepository<DiscountLevel> _levelRepo;
        private readonly IBaseRepository<Order> _orderRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IValidator<SaveDiscountLevelsDto> _saveValidator;

        public DiscountLevelService(IBaseRepository<DiscountLevel> levelRepo,
                                    IBaseRepository<Order> orderRepo,
                                    UserManager<AppUser> userManager,
                                    IValidator<SaveDiscountLevelsDto> saveValidator)
        {
            _levelRepo = levelRepo;
            _orderRepo = orderRepo;
            _userManager = userManager;
            _saveValidator = saveValidator;
        }

        public async Task<IEnumerable<DiscountLevelDto>> GetAllAsync()
        {
            var levels = (await _levelRepo.GetAllAsync()).OrderBy(m => m.MinSpent).ToList();
            var spent = await SpentByUserAsync();
            var customers = (await _userManager.GetUsersInRoleAsync(Roles.User)).Select(m => m.Id).ToList();

            // Hər müştəri xərclədiyi məbləğə uyğun ən yüksək səviyyəyə düşür
            var counts = customers
                .Select(id => LevelOf(levels, spent.GetValueOrDefault(id))?.Id)
                .Where(id => id is not null)
                .GroupBy(id => id!.Value)
                .ToDictionary(m => m.Key, m => m.Count());

            return levels.Select(m => new DiscountLevelDto
            {
                Id = m.Id,
                Name = m.Name,
                MinSpent = m.MinSpent,
                Percent = m.Percent,
                Perks = m.Perks,
                CustomerCount = counts.GetValueOrDefault(m.Id)
            }).ToList();
        }

        public async Task SaveAsync(SaveDiscountLevelsDto model)
        {
            await _saveValidator.EnsureValidAsync(model);

            var current = (await _levelRepo.GetAllAsync()).ToList();
            var keep = model.Levels.Where(m => m.Id is not null).Select(m => m.Id!.Value).ToHashSet();

            foreach (var level in current.Where(m => !keep.Contains(m.Id)))
            {
                await _levelRepo.DeleteAsync(level);
            }

            foreach (var item in model.Levels)
            {
                var level = item.Id is int id ? current.FirstOrDefault(m => m.Id == id) : null;
                if (level is null)
                {
                    await _levelRepo.AddAsync(new DiscountLevel
                    {
                        Name = item.Name!.Trim(),
                        MinSpent = item.MinSpent,
                        Percent = item.Percent,
                        Perks = string.IsNullOrWhiteSpace(item.Perks) ? null : item.Perks.Trim()
                    });
                    continue;
                }

                level.Name = item.Name!.Trim();
                level.MinSpent = item.MinSpent;
                level.Percent = item.Percent;
                level.Perks = string.IsNullOrWhiteSpace(item.Perks) ? null : item.Perks.Trim();
                await _levelRepo.UpdateAsync(level);
            }
        }

        public async Task<MyDiscountLevelDto> GetForUserAsync(string userId)
        {
            var levels = (await _levelRepo.GetAllAsync()).OrderBy(m => m.MinSpent).ToList();
            var spent = (await SpentByUserAsync()).GetValueOrDefault(userId);
            var level = LevelOf(levels, spent);
            var next = levels.FirstOrDefault(m => m.MinSpent > spent);

            return new MyDiscountLevelDto
            {
                Name = level?.Name,
                Percent = level?.Percent ?? 0,
                Spent = spent,
                NextName = next?.Name,
                NextMinSpent = next?.MinSpent
            };
        }

        // Ən yüksək səviyyə ki, başlanğıc məbləği xərclənən məbləğdən çox deyil
        private static DiscountLevel? LevelOf(List<DiscountLevel> levels, decimal spent)
        {
            return levels.LastOrDefault(m => m.MinSpent <= spent);
        }

        private async Task<Dictionary<string, decimal>> SpentByUserAsync()
        {
            return (await _orderRepo.GetAllAsync())
                .Where(m => m.Status == OrderStatus.Delivered)
                .GroupBy(m => m.UserId)
                .ToDictionary(m => m.Key, m => m.Sum(o => o.Total));
        }
    }
}
