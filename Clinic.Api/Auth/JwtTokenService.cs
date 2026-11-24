using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clinic.Api.Auth;

public sealed class JwtTokenService : ITokenService
{
    public const string DoctorIdClaim = "doctorId";

    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options) => _options = options.Value;

    public AccessTokenDto Create(AuthUserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
        };
        if (!string.IsNullOrEmpty(user.DoctorId))
            claims.Add(new Claim(DoctorIdClaim, user.DoctorId));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new AccessTokenDto(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
