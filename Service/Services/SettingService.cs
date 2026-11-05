using Domain.Entities;
using FluentValidation;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.Settings;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class SettingService : ISettingService
    {
        private readonly IBaseRepository<SiteSetting> _settingRepo;
        private readonly IValidator<UpdateSiteSettingDto> _updateValidator;

        public SettingService(IBaseRepository<SiteSetting> settingRepo, IValidator<UpdateSiteSettingDto> updateValidator)
        {
            _settingRepo = settingRepo;
            _updateValidator = updateValidator;
        }

        public async Task<SiteSettingDto> GetAsync()
        {
            var setting = (await _settingRepo.GetAllAsync()).FirstOrDefault();
            if (setting is null)
            {
                throw new NotFoundException();
            }

            return new SiteSettingDto
            {
                Phone = setting.Phone,
                Email = setting.Email,
                Address = setting.Address,
                Instagram = setting.Instagram,
                WhatsApp = setting.WhatsApp,
                Hours = setting.Hours
            };
        }

        public async Task UpdateAsync(UpdateSiteSettingDto model)
        {
            await _updateValidator.EnsureValidAsync(model);

            var existing = (await _settingRepo.GetAllAsync()).FirstOrDefault();
            var setting = existing is null ? null : await _settingRepo.GetByIdAsync(existing.Id);
            if (setting is null)
            {
                throw new NotFoundException();
            }

            setting.Phone = model.Phone!.Trim();
            setting.Email = model.Email!.Trim();
            setting.Address = model.Address!.Trim();
            setting.Instagram = model.Instagram!.Trim();
            setting.WhatsApp = model.WhatsApp!.Trim();
            setting.Hours = model.Hours!.Trim();
            await _settingRepo.UpdateAsync(setting);
        }
    }
}
