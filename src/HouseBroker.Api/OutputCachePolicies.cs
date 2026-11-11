namespace HouseBroker.Api;

/// <summary>
/// Names of output-cache policies. The built-in base policy only caches anonymous GET/HEAD
/// 200 responses, so a broker's response that contains commission is never cached.
/// </summary>
public static class OutputCachePolicies
{
    public const string Listings = "Listings";
}
