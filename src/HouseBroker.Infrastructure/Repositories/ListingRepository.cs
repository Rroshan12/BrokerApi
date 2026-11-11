using HouseBroker.Application.Common;
using HouseBroker.Application.Listings;
using HouseBroker.Domain.Entities;
using HouseBroker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBroker.Infrastructure.Repositories;

/// <summary>EF Core access to listings. Read queries are no-tracking and project straight to DTOs.</summary>
public sealed class ListingRepository(AppDbContext db) : IListingRepository
{
    public Task<PropertyListing?> GetByIdAsync(Guid id, bool track, CancellationToken ct)
    {
        var query = db.PropertyListings.Include(l => l.Images).AsQueryable();
        if (!track)
            query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<PagedResult<ListingSummaryDto>> SearchAsync(ListingSearchQuery q, CancellationToken ct)
    {
        var query = db.PropertyListings.AsNoTracking();

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
        foreach (var feature in (q.Features ?? []).Select(f => f.Trim()).Distinct())
            query = query.Where(l => l.Features.Contains(feature));

        var total = await query.CountAsync(ct);

        query = q.SortBy switch
        {
            ListingSortBy.PriceAsc => query.OrderBy(l => l.Price).ThenBy(l => l.Id),
            ListingSortBy.PriceDesc => query.OrderByDescending(l => l.Price).ThenBy(l => l.Id),
            _ => query.OrderByDescending(l => l.CreatedAtUtc).ThenBy(l => l.Id)
        };

        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(l => new ListingSummaryDto(
                l.Id, l.Title, l.PropertyType, l.Price, l.City, l.Area,
                l.Bedrooms, l.Bathrooms, l.AreaSqFt,
                l.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                l.CreatedAtUtc,
                null, null))
            .ToListAsync(ct);

        return new PagedResult<ListingSummaryDto>(items, q.Page, q.PageSize, total);
    }

    public async Task<PagedResult<ListingSummaryDto>> GetByBrokerAsync(string brokerId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.PropertyListings.AsNoTracking().Where(l => l.BrokerId == brokerId);
        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAtUtc).ThenBy(l => l.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new ListingSummaryDto(
                l.Id, l.Title, l.PropertyType, l.Price, l.City, l.Area,
                l.Bedrooms, l.Bathrooms, l.AreaSqFt,
                l.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).FirstOrDefault(),
                l.CreatedAtUtc,
                l.CommissionAmount, l.CommissionRatePercent))
            .ToListAsync(ct);

        return new PagedResult<ListingSummaryDto>(items, page, pageSize, total);
    }

    public void Add(PropertyListing listing) => db.PropertyListings.Add(listing);

    public void Remove(PropertyListing listing) => db.PropertyListings.Remove(listing);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
