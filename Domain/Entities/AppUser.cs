using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class AppUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Community-də görünən ad və səhifə ünvanı (/Community/User/{Nickname})
    public string? Nickname { get; set; }
    public string? AvatarPath { get; set; }

    // Hüquqi şəxs kimi alan müştəri üçün (fakturada istifadə olunur)
    public string? OrganizationName { get; set; }
    public string? OrganizationAddress { get; set; }
    public string? Voen { get; set; }
    public string? TaxNumber { get; set; }
}
