using HouseBroker.Domain.Enums;

namespace HouseBroker.Application.Listings;

/// <summary>Sort order for search results.</summary>
public enum ListingSortBy
{
    Newest = 0,
    PriceAsc = 1,
    PriceDesc = 2
}

/// <summary>
/// Search filters, bound from the query string. Every filter is optional and they combine with AND.
/// </summary>
public sealed record ListingSearchQuery
{
    /// <summary>Free text matched against city, area and address.</summary>
    public string? Location { get; init; }

    /// <summary>Exact city match.</summary>
    public string? City { get; init; }

    public PropertyType? PropertyType { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? MinBedrooms { get; init; }
    public int? MinBathrooms { get; init; }

    /// <summary>Listings must have all of these features (repeat the parameter: <c>features=Parking&amp;features=Garden</c>).</summary>
    public IReadOnlyList<string>? Features { get; init; }

    public ListingSortBy SortBy { get; init; } = ListingSortBy.Newest;
    public int Page { get; init; } = 1;

    /// <summary>Items per page, 1 to 50.</summary>
    public int PageSize { get; init; } = 10;
}
