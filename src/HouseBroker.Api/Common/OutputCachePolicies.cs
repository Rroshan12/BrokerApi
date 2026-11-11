namespace HouseBroker.Api.Common;

/// <summary>
/// Output-cache policies. The built-in base policy only caches anonymous GET/HEAD
/// 200 responses, so a broker's response that contains commission is never cached.
/// </summary>
public static class OutputCachePolicies
{
    public const string Listings = "Listings";

    /// <summary>How long a cached listing response lives if no write evicts it first.</summary>
    public static readonly TimeSpan ListingsTtl = TimeSpan.FromSeconds(60);
}
