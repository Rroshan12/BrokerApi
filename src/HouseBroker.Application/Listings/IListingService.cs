using HouseBroker.Application.Common;

namespace HouseBroker.Application.Listings;

/// <summary>Listing use cases: search, details, and broker-only create/update/delete.</summary>
public interface IListingService
{
    /// <summary>Public search with filters and paging.</summary>
    Task<PagedResult<ListingSummaryDto>> SearchAsync(ListingSearchQuery query, CancellationToken ct);

    /// <summary>Listing details, or null when not found. Commission is included only for the owning broker.</summary>
    Task<ListingDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>The calling broker's own listings, with commission.</summary>
    Task<PagedResult<ListingSummaryDto>> GetMineAsync(int page, int pageSize, CancellationToken ct);

    /// <summary>Creates a listing owned by the calling broker and calculates its commission.</summary>
    Task<ListingDetailsDto> CreateAsync(ListingRequest request, CancellationToken ct);

    /// <summary>Replaces a listing. Only its owner may do this; commission is recalculated.</summary>
    Task UpdateAsync(Guid id, ListingRequest request, CancellationToken ct);

    /// <summary>Deletes a listing. Only its owner may do this.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct);
}
