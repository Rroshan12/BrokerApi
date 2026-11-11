using FluentValidation;
using HouseBroker.Application.Commission;
using HouseBroker.Application.Common;
using HouseBroker.Application.Common.Interfaces;
using HouseBroker.Application.Common.Pagination;
using HouseBroker.Domain.Entities;
using HouseBroker.Domain.Enums;

namespace HouseBroker.Application.Listings;

/// <summary>
/// Listing use cases. Owns the business rules: only brokers manage listings, only the owner
/// may change one, commission is recalculated on every write and shown only to the owner.
/// </summary>
public sealed class ListingService(
    IListingRepository repository,
    ICommissionCalculator commissionCalculator,
    IBrokerDirectory brokerDirectory,
    ICurrentUserService currentUser,
    ICacheInvalidator cacheInvalidator,
    TimeProvider time,
    IValidator<ListingRequest> listingValidator,
    IValidator<ListingSearchQuery> searchValidator,
    IValidator<PageRequest> pageValidator) : IListingService
{
    /// <inheritdoc />
    public async Task<PagedResult<ListingSummaryDto>> SearchAsync(ListingSearchQuery query, CancellationToken ct)
    {
        await searchValidator.ValidateAndThrowAsync(query, ct);
        return await repository.SearchAsync(query, ct);
    }

    /// <inheritdoc />
    public async Task<ListingDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var listing = await repository.GetByIdAsync(id, track: false, ct);
        if (listing is null)
            return null;

        var broker = await brokerDirectory.GetContactAsync(listing.BrokerId, ct);
        return listing.ToDetailsDto(broker, includeCommission: listing.IsOwnedBy(currentUser.UserId));
    }

    /// <inheritdoc />
    public async Task<PagedResult<ListingSummaryDto>> GetMineAsync(PageRequest paging, CancellationToken ct)
    {
        var brokerId = RequireBroker();
        await pageValidator.ValidateAndThrowAsync(paging, ct);
        return await repository.GetByBrokerAsync(brokerId, paging, ct);
    }

    /// <inheritdoc />
    public async Task<ListingDetailsDto> CreateAsync(ListingRequest request, CancellationToken ct)
    {
        var brokerId = RequireBroker();
        await listingValidator.ValidateAndThrowAsync(request, ct);

        var listing = new PropertyListing { BrokerId = brokerId, CreatedAtUtc = UtcNow() };
        listing.Apply(request);
        await ApplyCommissionAsync(listing, ct);

        repository.Add(listing);
        await SaveAndEvictAsync(ct);

        var broker = await brokerDirectory.GetContactAsync(brokerId, ct);
        return listing.ToDetailsDto(broker, includeCommission: true);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Guid id, ListingRequest request, CancellationToken ct)
    {
        var brokerId = RequireBroker();
        await listingValidator.ValidateAndThrowAsync(request, ct);

        var listing = await GetOwnedListingAsync(id, brokerId, ct);
        listing.Apply(request);
        // Always recalculate: the price may have changed, or the rates in the DB may have.
        await ApplyCommissionAsync(listing, ct);
        listing.MarkUpdated(UtcNow());

        await SaveAndEvictAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var brokerId = RequireBroker();
        var listing = await GetOwnedListingAsync(id, brokerId, ct);

        repository.Remove(listing);
        await SaveAndEvictAsync(ct);
    }

    private async Task ApplyCommissionAsync(PropertyListing listing, CancellationToken ct)
    {
        var commission = await commissionCalculator.CalculateAsync(listing.Price, ct);
        listing.SetCommission(commission.RatePercent, commission.Amount);
    }

    /// <summary>Commits the change, then drops cached listing responses so readers see it.</summary>
    private async Task SaveAndEvictAsync(CancellationToken ct)
    {
        await repository.SaveChangesAsync(ct);
        await cacheInvalidator.EvictByTagAsync(CacheTags.Listings, ct);
    }

    private async Task<PropertyListing> GetOwnedListingAsync(Guid id, string brokerId, CancellationToken ct)
    {
        var listing = await repository.GetByIdAsync(id, track: true, ct)
            ?? throw new KeyNotFoundException($"Listing {id} was not found.");

        if (!listing.IsOwnedBy(brokerId))
            throw new UnauthorizedAccessException("Only the broker who owns this listing can change it.");

        return listing;
    }

    /// <summary>Defence in depth: controllers already require the Broker role.</summary>
    private string RequireBroker() =>
        currentUser.UserId is { } id && currentUser.IsInRole(Roles.Broker)
            ? id
            : throw new UnauthorizedAccessException("Only brokers can manage listings.");

    private DateTime UtcNow() => time.GetUtcNow().UtcDateTime;
}
