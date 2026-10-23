using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Repository.Data;

// İşçi hesablarını yaradır ki, boş bazada da admin panelə daxil olmaq mümkün olsun (ilk SuperAdmin buradan gəlir).
// Hesab artıq varsa parol toxunulmaz qalır, yalnız rolu appsettings-dəki ilə eyni edilir.
public static class AdminSeeder
{
    public static async Task SeedAsync(UserManager<AppUser> userManager,
                                       IEnumerable<SeedAdminSettings> admins,
                                       ILogger logger)
    {
        foreach (var admin in admins)
        {
            if (string.IsNullOrWhiteSpace(admin.Email) || string.IsNullOrWhiteSpace(admin.Password))
            {
                logger.LogWarning("SeedAdmins: email və ya parol boşdur, sətir atlandı.");
                continue;
            }

            if (!Roles.Staff.Contains(admin.Role))
            {
                logger.LogWarning("SeedAdmins: {Email} üçün rol tanınmadı: {Role}", admin.Email, admin.Role);
                continue;
            }

            var user = await userManager.FindByEmailAsync(admin.Email);
            if (user is null)
            {
                user = new AppUser
                {
                    Name = string.IsNullOrWhiteSpace(admin.Name) ? admin.Role : admin.Name,
                    Surname = string.IsNullOrWhiteSpace(admin.Surname) ? "Janz" : admin.Surname,
                    Email = admin.Email,
                    UserName = admin.Email,                 // login email ilədir
                    EmailConfirmed = true
                };

                var created = await userManager.CreateAsync(user, admin.Password);
                if (!created.Succeeded)
                {
                    // Parol qaydaya uyğun deyilsə proqram dayanmasın, səbəb log-a yazılsın
                    logger.LogWarning("SeedAdmins: {Email} yaradıla bilmədi: {Errors}",
                        admin.Email, string.Join("; ", created.Errors.Select(e => e.Description)));
                    continue;
                }
            }

            var current = await userManager.GetRolesAsync(user);
            if (current.Count == 1 && current[0] == admin.Role)
            {
                continue;
            }

            // Hər istifadəçinin bir rolu olur: köhnə rollar silinir, appsettings-dəki verilir
            if (current.Count > 0)
            {
                await userManager.RemoveFromRolesAsync(user, current);
            }

            var roleResult = await userManager.AddToRoleAsync(user, admin.Role);
            if (!roleResult.Succeeded)
            {
                logger.LogWarning("SeedAdmins: {Email} üçün rol verilmədi: {Errors}",
                    admin.Email, string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
