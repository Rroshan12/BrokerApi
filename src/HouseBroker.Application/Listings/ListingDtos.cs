using HouseBroker.Domain.Enums;

namespace HouseBroker.Application.Listings;

/// <summary>
/// Body for creating or fully replacing a listing (POST and PUT share it).
/// Images are given as absolute http(s) URLs.
/// </summary>
public sealed record ListingRequest(
    string Title,
    string Description,
    PropertyType PropertyType,
    decimal Price,
    string City,
    string Area,
    string Address,
    int Bedrooms,
    int Bathrooms,
    decimal AreaSqFt,
    IReadOnlyList<string>? Features,
    IReadOnlyList<string>? ImageUrls);

/// <summary>Public contact details of the broker who owns a listing.</summary>
public sealed record BrokerContactDto(string Id, string FullName, string? AgencyName, string Email, string? PhoneNumber);

/// <summary>
/// A listing in search results. Commission fields are filled only in the owning
/// broker's own list (<c>GET listings/mine</c>) and are omitted from JSON when null.
/// </summary>
public sealed record ListingSummaryDto(
    Guid Id,
    string Title,
    PropertyType PropertyType,
    decimal Price,
    string City,
    string Area,
    int Bedrooms,
    int Bathrooms,
    decimal AreaSqFt,
    string? ThumbnailUrl,
    DateTime CreatedAtUtc,
    decimal? CommissionAmount = null,
    decimal? CommissionRatePercent = null);

/// <summary>
/// Full listing details including images and broker contact.
/// Commission fields are null (and omitted from JSON) unless the caller owns the listing.
/// </summary>
public sealed record ListingDetailsDto(
    Guid Id,
    string Title,
    string Description,
    PropertyType PropertyType,
    decimal Price,
    string City,
    string Area,
    string Address,
    int Bedrooms,
    int Bathrooms,
    decimal AreaSqFt,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> ImageUrls,
    BrokerContactDto? Broker,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    decimal? CommissionAmount,
    decimal? CommissionRatePercent);
