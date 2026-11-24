using Clinic.Application.DTOs.Auth;

namespace Clinic.Application.Abstractions;

public interface IAuthService
{
    /// <summary>Signs a staff member in. Returns null when the credentials are refused.</summary>
    Task<LoginResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct);

    /// <summary>Profile of the signed-in user, or null when the account is gone or deactivated.</summary>
    Task<AuthUserDto?> GetProfileAsync(string userId, CancellationToken ct);
}
