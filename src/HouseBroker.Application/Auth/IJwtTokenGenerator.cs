namespace HouseBroker.Application.Auth;

/// <summary>Issues signed JWTs. Implemented in Infrastructure.</summary>
public interface IJwtTokenGenerator
{
    AccessToken Generate(AuthenticatedUser user);
}
