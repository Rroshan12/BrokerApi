using FluentValidation;
using HouseBroker.Application.Common.Exceptions;
using HouseBroker.Domain.Enums;

namespace HouseBroker.Application.Auth;

/// <summary>
/// Registration and login. The chosen <see cref="AccountType"/> becomes an Identity role
/// (Seeker or Broker) and is carried in the JWT, which is how the API tells them apart.
/// </summary>
public sealed class AuthService(
    IIdentityService identityService,
    IJwtTokenGenerator tokenGenerator,
    IValidator<RegisterRequest> registerValidator,
    IValidator<LoginRequest> loginValidator) : IAuthService
{
    /// <inheritdoc />
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        await registerValidator.ValidateAndThrowAsync(request, ct);

        var isBroker = request.AccountType == AccountType.Broker;
        var role = isBroker ? Roles.Broker : Roles.Seeker;
        var newUser = new NewUser(
            request.Email.Trim(),
            request.FullName.Trim(),
            NullIfBlank(request.PhoneNumber),
            isBroker ? NullIfBlank(request.AgencyName) : null);

        var user = await identityService.CreateUserAsync(newUser, request.Password, role, ct);
        return ToResponse(user);
    }

    /// <inheritdoc />
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        await loginValidator.ValidateAndThrowAsync(request, ct);

        var user = await identityService.ValidateCredentialsAsync(request.Email.Trim(), request.Password, ct)
            ?? throw new InvalidCredentialsException();
        return ToResponse(user);
    }

    private AuthResponse ToResponse(AuthenticatedUser user)
    {
        var token = tokenGenerator.Generate(user);
        return new AuthResponse(token.Token, token.ExpiresAtUtc, user.Id, user.Email, user.FullName, user.Role);
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
