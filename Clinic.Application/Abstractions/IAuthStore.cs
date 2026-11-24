using Clinic.Application.DTOs.Auth;

namespace Clinic.Application.Abstractions;

/// <summary>Port implemented in Infrastructure with ASP.NET Core Identity.</summary>
public interface IAuthStore
{
    /// <summary>
    /// Checks the credentials and returns the user, or null when sign-in must be refused
    /// (unknown email, wrong password, inactive or locked-out account). Failed attempts count towards lockout.
    /// </summary>
    Task<AuthUserDto?> ValidateCredentialsAsync(string email, string password, CancellationToken ct);

    /// <summary>The user with this id, only if the account still exists and is active.</summary>
    Task<AuthUserDto?> FindActiveAsync(string userId, CancellationToken ct);
}
