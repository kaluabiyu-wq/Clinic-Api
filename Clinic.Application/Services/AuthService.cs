using Clinic.Application.Abstractions;
using Clinic.Application.DTOs.Auth;
using FluentValidation;

namespace Clinic.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAuthStore _store;
    private readonly ITokenService _tokens;
    private readonly IValidator<LoginDto> _validator;

    public AuthService(IAuthStore store, ITokenService tokens, IValidator<LoginDto> validator)
    {
        _store = store;
        _tokens = tokens;
        _validator = validator;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(dto, ct);

        // One null for every failure, so callers cannot tell which emails exist.
        var user = await _store.ValidateCredentialsAsync(dto.Email.Trim(), dto.Password, ct);
        if (user is null) return null;

        var token = _tokens.Create(user);
        return new LoginResponseDto(token.Token, token.ExpiresAtUtc, user);
    }

    public Task<AuthUserDto?> GetProfileAsync(string userId, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(userId)
            ? Task.FromResult<AuthUserDto?>(null)
            : _store.FindActiveAsync(userId, ct);
}
