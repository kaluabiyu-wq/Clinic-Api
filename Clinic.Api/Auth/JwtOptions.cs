namespace Clinic.Api.Auth;

/// <summary>Bound from the "Jwt" section of appsettings.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "clinic-api";
    public string Audience { get; set; } = "clinic-client";

    /// <summary>Signing key, at least 32 characters. Override outside development (env var Jwt__Key).</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 480;
}
