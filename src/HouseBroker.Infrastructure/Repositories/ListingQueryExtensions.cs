using HouseBroker.Application.Listings;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Infrastructure.Repositories;

/// <summary>Translates search filters and sort order into EF Core LINQ (SQL WHERE / ORDER BY).</summary>
public static class ListingQueryExtensions
{
    public static IQueryable<PropertyListing> ApplyFilters(this IQueryable<PropertyListing> query, ListingSearchQuery q)
    {
        if (!string.IsNullOrWhiteSpace(q.City))
        {
            var city = q.City.Trim();
            query = query.Where(l => l.City == city);
        }
        if (!string.IsNullOrWhiteSpace(q.Location))
        {
            var location = q.Location.Trim();
            query = query.Where(l => l.City.Contains(location) || l.Area.Contains(location) || l.Address.Contains(location));
        }
        if (q.PropertyType is { } type)
            query = query.Where(l => l.PropertyType == type);
        if (q.MinPrice is { } minPrice)
            query = query.Where(l => l.Price >= minPrice);
        if (q.MaxPrice is { } maxPrice)
            query = query.Where(l => l.Price <= maxPrice);
        if (q.MinBedrooms is { } beds)
            query = query.Where(l => l.Bedrooms >= beds);
        if (q.MinBathrooms is { } baths)
            query = query.Where(l => l.Bathrooms >= baths);
        foreach (var feature in (q.Features ?? []).Select(f => f.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
            query = query.Where(l => l.Features.Contains(feature));

        return query;
    }

    /// <summary>Orders results; Id is the tie-breaker so paging is stable.</summary>
    public static IQueryable<PropertyListing> ApplySorting(this IQueryable<PropertyListing> query, ListingSortBy sortBy) =>
        sortBy switch
        {
            ListingSortBy.PriceAsc => query.OrderBy(l => l.Price).ThenBy(l => l.Id),
            ListingSortBy.PriceDesc => query.OrderByDescending(l => l.Price).ThenBy(l => l.Id),
            _ => query.OrderByDescending(l => l.CreatedAtUtc).ThenBy(l => l.Id)
        };
}
