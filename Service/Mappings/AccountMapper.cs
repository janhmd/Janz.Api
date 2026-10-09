using Domain.Entities;
using Service.DTOs.Account;

namespace Service.Mappings;

public static class AccountMapper
{
    public static AppUser ToEntity(RegisterDto dto)
    {
        SplitName(dto.FullName, out var name, out var surname);

        return new AppUser
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            Name = name,
            Surname = surname
        };
    }

    public static AuthResponseDto ToResponse(AppUser user, string token, string role)
    {
        var fullName = string.Join(" ", new[] { user.Name, user.Surname }.Where(part => !string.IsNullOrWhiteSpace(part)));

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            FullName = fullName,
            Role = role
        };
    }

    private static void SplitName(string fullName, out string name, out string surname)
    {
        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        name = parts.Length > 0 ? parts[0] : string.Empty;
        surname = parts.Length > 1 ? parts[1] : string.Empty;
    }
}
