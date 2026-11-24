using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace Clinic.Infrastructure.Identity;

public class AuthStore : IAuthStore
{
    private readonly UserManager<ApplicationUser> _users;

    public AuthStore(UserManager<ApplicationUser> users) => _users = users;

    public async Task<AuthUserDto?> ValidateCredentialsAsync(string email, string password, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user is null || !user.IsActive) return null;

        if (await _users.IsLockedOutAsync(user)) return null;

        if (!await _users.CheckPasswordAsync(user, password))
        {
            await _users.AccessFailedAsync(user);   // counts towards lockout
            return null;
        }

        await _users.ResetAccessFailedCountAsync(user);
        return await ToDtoAsync(user);
    }

    public async Task<AuthUserDto?> FindActiveAsync(string userId, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId);
        return user is { IsActive: true } ? await ToDtoAsync(user) : null;
    }

    private async Task<AuthUserDto> ToDtoAsync(ApplicationUser u)
    {
        var roles = await _users.GetRolesAsync(u);
        return new AuthUserDto(u.Id, u.FullName, u.Email ?? string.Empty, roles.FirstOrDefault() ?? string.Empty, u.DoctorId);
    }
}
