using HouseBroker.Application.Common.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;

namespace HouseBroker.Api.Services;

/// <summary>Reads the caller's id and roles from the validated JWT on the current request.</summary>
public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string? UserId
    {
        get
        {
            var user = accessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true ? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value : null;
        }
    }

    public bool IsInRole(string role) => accessor.HttpContext?.User.IsInRole(role) ?? false;
}
