namespace HouseBroker.Application.Auth;

/// <summary>Registration and login for house seekers and brokers.</summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
}
