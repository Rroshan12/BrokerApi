using HouseBroker.Application.Common;
using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Listings;

/// <summary>Persistence for property listings. Implemented with EF Core in Infrastructure.</summary>
public interface IListingRepository
{
    /// <summary>Loads a listing with its images. Use <paramref name="track"/> = true when it will be modified.</summary>
    Task<PropertyListing?> GetByIdAsync(Guid id, bool track, CancellationToken ct);

    /// <summary>Public search. Never fills commission fields.</summary>
    Task<PagedResult<ListingSummaryDto>> SearchAsync(ListingSearchQuery query, CancellationToken ct);

    /// <summary>A broker's own listings, newest first, with commission filled in.</summary>
    Task<PagedResult<ListingSummaryDto>> GetByBrokerAsync(string brokerId, int page, int pageSize, CancellationToken ct);

    void Add(PropertyListing listing);
    void Remove(PropertyListing listing);
    Task SaveChangesAsync(CancellationToken ct);
}
