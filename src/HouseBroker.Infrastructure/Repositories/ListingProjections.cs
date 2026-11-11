using System.Linq.Expressions;
using HouseBroker.Application.Listings;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Infrastructure.Repositories;

/// <summary>
/// SQL-translatable projections from listings to summary DTOs, so only the needed columns
/// (and the first image) are read. Commission is selected only in the owner projection.
/// </summary>
public static class ListingProjections
{
    /// <summary>For public search: commission is never selected.</summary>
    public static readonly Expression<Func<PropertyListing, ListingSummaryDto>> PublicSummary = l =>
        new ListingSummaryDto(
            l.Id, l.Title, l.PropertyType, l.Price, l.City, l.Area,
            l.Bedrooms, l.Bathrooms, l.AreaSqFt,
            l.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
            l.CreatedAtUtc,
            null, null);

    /// <summary>For the owning broker's own list: includes commission.</summary>
    public static readonly Expression<Func<PropertyListing, ListingSummaryDto>> OwnerSummary = l =>
        new ListingSummaryDto(
            l.Id, l.Title, l.PropertyType, l.Price, l.City, l.Area,
            l.Bedrooms, l.Bathrooms, l.AreaSqFt,
            l.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
            l.CreatedAtUtc,
            l.CommissionAmount, l.CommissionRatePercent);
}
