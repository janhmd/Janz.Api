using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Repository.Repositories.Interfaces;
using Service.Helpers.DTOs.GiftCertificates;
using Service.Helpers.Exceptions;
using Service.Helpers.Validators;
using Service.Services.Interfaces;

namespace Service.Services
{
    public class GiftCertificateService : IGiftCertificateService
    {
        private readonly IBaseRepository<GiftCertificate> _certificateRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserAdminService _userService;
        private readonly INotificationService _notificationService;
        private readonly IValidator<CreateGiftCertificateDto> _createValidator;
        private readonly IValidator<UpdateGiftCertificateDto> _updateValidator;

        public GiftCertificateService(IBaseRepository<GiftCertificate> certificateRepo,
                                      UserManager<AppUser> userManager,
                                      IUserAdminService userService,
                                      INotificationService notificationService,
                                      IValidator<CreateGiftCertificateDto> createValidator,
                                      IValidator<UpdateGiftCertificateDto> updateValidator)
        {
            _certificateRepo = certificateRepo;
            _userManager = userManager;
            _userService = userService;
            _notificationService = notificationService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        public async Task<IEnumerable<GiftCertificateDto>> GetAllAsync(string? status)
        {
            var result = new List<GiftCertificateDto>();
            foreach (var certificate in await _certificateRepo.GetAllAsync())
            {
                result.Add(await ToDtoAsync(certificate));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                result = result.Where(m => string.Equals(m.Status, status.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return result;
        }

        public async Task<GiftCertificateDto> GetByIdAsync(int id)
        {
            var certificate = await _certificateRepo.GetByIdAsync(id);
            if (certificate is null)
            {
                throw new NotFoundException();
            }

            return await ToDtoAsync(certificate);
        }

        public async Task<GiftCertificateDto> CreateAsync(CreateGiftCertificateDto model)
        {
            await _createValidator.EnsureValidAsync(model);

            var code = model.Code!.Trim().ToUpperInvariant();
            await EnsureCodeIsFreeAsync(code, null);

            var certificate = new GiftCertificate
            {
                Code = code,
                Amount = Math.Round(model.Amount, 2),
                ExpiresAt = model.ExpiresAt?.Date,
                IsActive = model.IsActive
            };
            await _certificateRepo.AddAsync(certificate);

            return await ToDtoAsync(certificate);
        }

        public async Task UpdateAsync(int id, UpdateGiftCertificateDto model)
        {
            await _updateValidator.EnsureValidAsync(model);

            var certificate = await _certificateRepo.GetByIdAsync(id);
            if (certificate is null)
            {
                throw new NotFoundException();
            }

            // İstifadə olunmuş sertifikatın kodu və məbləği artıq balansdadır, dəyişmək olmaz
            var code = model.Code!.Trim().ToUpperInvariant();
            var amount = Math.Round(model.Amount, 2);
            if (certificate.UsedById is not null && (code != certificate.Code || amount != certificate.Amount))
            {
                throw new BadRequestException("This certificate is already used: its code and amount can't be changed.");
            }

            await EnsureCodeIsFreeAsync(code, id);

            certificate.Code = code;
            certificate.Amount = amount;
            certificate.ExpiresAt = model.ExpiresAt?.Date;
            certificate.IsActive = model.IsActive;
            await _certificateRepo.UpdateAsync(certificate);
        }

        public async Task DeleteAsync(int id)
        {
            var certificate = await _certificateRepo.GetByIdAsync(id);
            if (certificate is null)
            {
                throw new NotFoundException();
            }

            if (certificate.UsedById is not null)
            {
                throw new BadRequestException("This certificate is already used, so it can't be deleted. Disable it instead.");
            }

            await _certificateRepo.DeleteAsync(certificate);
        }

        public async Task<int> NotifyAsync(int id, NotifyGiftCertificateDto model)
        {
            var certificate = await _certificateRepo.GetByIdAsync(id);
            if (certificate is null)
            {
                throw new NotFoundException();
            }

            var status = StatusOf(certificate);
            if (status != "Active")
            {
                throw new BadRequestException($"This certificate can't be sent now: it is {status.ToLowerInvariant()}.");
            }

            var all = string.Equals(model.Audience, "All", StringComparison.OrdinalIgnoreCase);
            var selected = string.Equals(model.Audience, "Selected", StringComparison.OrdinalIgnoreCase);
            if (!all && !selected)
            {
                throw new BadRequestException("Choose who gets the certificate.");
            }

            // Yalnız bloklanmamış müştərilər; seçilmişlərdə bilinməyən və ya işçi hesabları atılır
            var customers = (await _userService.GetAllAsync(Roles.User, false, null)).Select(m => m.Id).ToHashSet();
            var recipients = all
                ? customers.ToList()
                : (model.UserIds ?? new List<string>()).Where(customers.Contains).Distinct().ToList();
            if (recipients.Count == 0)
            {
                throw new BadRequestException("Choose at least one customer.");
            }

            var text = $"Enter the code {certificate.Code} in your account under Internal account and ₼{certificate.Amount:0.00} goes to your balance"
                + (certificate.ExpiresAt is null ? "." : $" (valid until {certificate.ExpiresAt.Value:dd.MM.yyyy}).");
            foreach (var userId in recipients)
            {
                await _notificationService.AddAsync(userId, NotificationKind.Shop, "A gift certificate for you.", text, "/Profile#ledger");
            }

            return recipients.Count;
        }

        private async Task EnsureCodeIsFreeAsync(string code, int? exceptId)
        {
            var taken = (await _certificateRepo.GetAllAsync()).Any(m => m.Code == code && m.Id != exceptId);
            if (taken)
            {
                throw new BadRequestException("This code is already used by another certificate.");
            }
        }

        private async Task<GiftCertificateDto> ToDtoAsync(GiftCertificate certificate)
        {
            string? email = null;
            if (certificate.UsedById is not null)
            {
                email = (await _userManager.FindByIdAsync(certificate.UsedById))?.Email;
            }

            return new GiftCertificateDto
            {
                Id = certificate.Id,
                Code = certificate.Code,
                Amount = certificate.Amount,
                ExpiresAt = certificate.ExpiresAt,
                IsActive = certificate.IsActive,
                Status = StatusOf(certificate),
                UsedByEmail = email,
                UsedAt = certificate.UsedAt,
                CreatedAt = certificate.CreatedAt
            };
        }

        private static string StatusOf(GiftCertificate certificate)
        {
            if (certificate.UsedById is not null) return "Used";
            if (!certificate.IsActive) return "Disabled";
            if (certificate.ExpiresAt is not null && certificate.ExpiresAt <= DateTime.UtcNow) return "Expired";
            return "Active";
        }
    }
}
