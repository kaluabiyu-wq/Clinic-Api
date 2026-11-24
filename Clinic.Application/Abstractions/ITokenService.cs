using Clinic.Application.DTOs.Auth;

namespace Clinic.Application.Abstractions;

/// <summary>Port that signs access tokens (JWT in the API project).</summary>
public interface ITokenService
{
    AccessTokenDto Create(AuthUserDto user);
}
