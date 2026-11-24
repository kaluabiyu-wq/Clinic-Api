using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ICurrentUser _current;

    public AuthController(IAuthService auth, ICurrentUser current)
    {
        _auth = auth;
        _current = current;
    }

    /// <summary>Staff sign-in. Returns a JWT to send as "Authorization: Bearer ...".</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(dto, ct);
        if (result is null)
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid email or password."
            });

        return Ok(result);
    }

    /// <summary>The signed-in user's profile (used to restore a session).</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var user = await _auth.GetProfileAsync(_current.UserId ?? string.Empty, ct);
        return user is null ? Unauthorized() : Ok(user);
    }
}
