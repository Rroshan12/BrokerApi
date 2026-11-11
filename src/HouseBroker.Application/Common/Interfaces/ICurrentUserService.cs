namespace HouseBroker.Application.Common.Interfaces;

/// <summary>The caller of the current request. Implemented in the Api layer from the JWT.</summary>
public interface ICurrentUserService
{
    /// <summary>Identity user id, or null for anonymous callers.</summary>
    string? UserId { get; }

    bool IsInRole(string role);
}
