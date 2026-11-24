using System.ComponentModel.DataAnnotations;
using Clinic.Api.Auth;
using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

public sealed record LoginRequest([Required] string Email, [Required] string Password);

public sealed record AuthUserDto(string Id, string FullName, string Email, string Role, string? DoctorId);

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc, AuthUserDto User);

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly JwtTokenService _tokens;

    public AuthController(UserManager<ApplicationUser> users, JwtTokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    /// <summary>Staff sign-in. Returns a JWT to send as "Authorization: Bearer ...".</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(request.Email.Trim());

        // Same answer for every failure so the API does not reveal which emails exist.
        if (user is null
            || !user.IsActive
            || await _users.IsLockedOutAsync(user)
            || !await _users.CheckPasswordAsync(user, request.Password))
        {
            if (user is not null && user.IsActive)
                await _users.AccessFailedAsync(user);

            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid email or password."
            });
        }

        await _users.ResetAccessFailedCountAsync(user);

        var roles = await _users.GetRolesAsync(user);
        var (token, expires) = _tokens.Create(user, roles);

        return Ok(new LoginResponse(token, expires, ToDto(user, roles)));
    }

    /// <summary>The signed-in user's profile (used to restore a session).</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me()
    {
        var user = await _users.GetUserAsync(User);
        if (user is null || !user.IsActive) return Unauthorized();

        return Ok(ToDto(user, await _users.GetRolesAsync(user)));
    }

    private static AuthUserDto ToDto(ApplicationUser u, IList<string> roles) =>
        new(u.Id, u.FullName, u.Email ?? string.Empty, roles.FirstOrDefault() ?? string.Empty, u.DoctorId);
}