namespace HouseBroker.Api;

/// <summary>Names of rate-limit policies.</summary>
public static class RateLimitPolicies
{
    /// <summary>Per-IP limit on register/login to slow down password guessing.</summary>
    public const string Auth = "auth";
}
