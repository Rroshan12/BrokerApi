namespace HouseBroker.Application.Common;

/// <summary>Output-cache tags shared by the Api (tagging) and services (eviction).</summary>
public static class CacheTags
{
    /// <summary>Every cached listing search and listing detail response.</summary>
    public const string Listings = "listings";
}
