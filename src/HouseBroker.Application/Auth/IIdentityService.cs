namespace HouseBroker.Application.Auth;

/// <summary>User store operations, implemented with ASP.NET Identity in Infrastructure.</summary>
public interface IIdentityService
{
    /// <summary>Creates the user in the given role. Throws ConflictException if the email is taken.</summary>
    Task<AuthenticatedUser> CreateUserAsync(NewUser user, string password, string role, CancellationToken ct);

    /// <summary>Returns the user when the password is correct and the account is not locked out; otherwise null.</summary>
    Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password, CancellationToken ct);
}
