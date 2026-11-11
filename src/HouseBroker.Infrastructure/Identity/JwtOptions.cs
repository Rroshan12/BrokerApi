namespace HouseBroker.Infrastructure.Identity;

/// <summary>
/// JWT settings bound from the "Jwt" configuration section.
/// <see cref="Key"/> must come from user-secrets (dev) or an environment variable (prod).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Claim type that carries the user's role in issued tokens.</summary>
    public const string RoleClaimType = "role";

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "HouseBroker";
    public string Audience { get; set; } = "HouseBroker.Clients";
    public int ExpiryMinutes { get; set; } = 60;
}
