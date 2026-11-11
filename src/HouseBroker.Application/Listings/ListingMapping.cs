using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Listings;

/// <summary>Maps between listing entities and DTOs. Commission is copied only when asked to.</summary>
public static class ListingMapping
{
    /// <summary>Copies request fields onto the entity, trimming text and de-duplicating features.</summary>
    public static void Apply(this PropertyListing listing, ListingRequest request)
    {
        listing.Title = request.Title.Trim();
        listing.Description = request.Description.Trim();
        listing.PropertyType = request.PropertyType;
        listing.Price = request.Price;
        listing.City = request.City.Trim();
        listing.Area = request.Area.Trim();
        listing.Address = request.Address.Trim();
        listing.Bedrooms = request.Bedrooms;
        listing.Bathrooms = request.Bathrooms;
        listing.AreaSqFt = request.AreaSqFt;
        listing.Features = (request.Features ?? [])
            .Select(f => f.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        listing.ReplaceImages((request.ImageUrls ?? []).Select(u => u.Trim()));
    }

    public static ListingDetailsDto ToDetailsDto(this PropertyListing l, BrokerContactDto? broker, bool includeCommission) => new(
        l.Id, l.Title, l.Description, l.PropertyType, l.Price, l.City, l.Area, l.Address,
        l.Bedrooms, l.Bathrooms, l.AreaSqFt,
        [.. l.Features],
        [.. l.Images.OrderBy(i => i.SortOrder).Select(i => i.Url)],
        broker,
        l.CreatedAtUtc, l.UpdatedAtUtc,
        includeCommission ? l.CommissionAmount : null,
        includeCommission ? l.CommissionRatePercent : null);
}
