using HouseBroker.Application.Common.Pagination;
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

    public Task<PagedResult<ListingSummaryDto>> SearchAsync(ListingSearchQuery query, CancellationToken ct) =>
        db.PropertyListings
            .AsNoTracking()
            .ApplyFilters(query)
            .ApplySorting(query.SortBy)
            .Select(ListingProjections.PublicSummary)
            .ToPagedResultAsync(query, ct);

    public Task<PagedResult<ListingSummaryDto>> GetByBrokerAsync(string brokerId, PageRequest paging, CancellationToken ct) =>
        db.PropertyListings
            .AsNoTracking()
            .Where(l => l.BrokerId == brokerId)
            .ApplySorting(ListingSortBy.Newest)
            .Select(ListingProjections.OwnerSummary)
            .ToPagedResultAsync(paging, ct);

    public void Add(PropertyListing listing) => db.PropertyListings.Add(listing);

    public void Remove(PropertyListing listing) => db.PropertyListings.Remove(listing);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
