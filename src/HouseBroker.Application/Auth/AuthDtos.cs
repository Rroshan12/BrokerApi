namespace HouseBroker.Application.Auth;

/// <summary>Which kind of account to create. This is how seekers and brokers are told apart.</summary>
public enum AccountType
{
    Seeker = 1,
    Broker = 2
}

/// <summary>Sign-up body. Brokers must give a phone number because it is shown on their listings.</summary>
public sealed record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    AccountType AccountType,
    string? PhoneNumber,
    string? AgencyName);

public sealed record LoginRequest(string Email, string Password);

/// <summary>Returned after register or login. Send the token as <c>Authorization: Bearer &lt;token&gt;</c>.</summary>
public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string UserId,
    string Email,
    string FullName,
    string Role);

/// <summary>Data for a new Identity user.</summary>
public sealed record NewUser(string Email, string FullName, string? PhoneNumber, string? AgencyName);

/// <summary>A user whose credentials were verified.</summary>
public sealed record AuthenticatedUser(string Id, string Email, string FullName, string Role);

/// <summary>A signed JWT and when it expires.</summary>
public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);
