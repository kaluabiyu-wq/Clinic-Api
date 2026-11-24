namespace Clinic.Application.DTOs.Auth;

/// <summary>Body of POST /auth/login. Checked by <c>LoginValidator</c>.</summary>
public sealed record LoginDto(string Email, string Password);

/// <summary>The signed-in staff member as the client sees it.</summary>
public sealed record AuthUserDto(
    string Id,
    string FullName,
    string Email,
    string Role,
    string? DoctorId);

/// <summary>A signed access token and the moment it stops working.</summary>
public sealed record AccessTokenDto(string Token, DateTime ExpiresAtUtc);

/// <summary>Response of POST /auth/login.</summary>
public sealed record LoginResponseDto(string Token, DateTime ExpiresAtUtc, AuthUserDto User);
