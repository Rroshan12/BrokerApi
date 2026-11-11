using HouseBroker.Domain.Common;
using HouseBroker.Domain.Enums;

namespace HouseBroker.Domain.Entities;

/// <summary>A property put up for sale by a broker.</summary>
public class PropertyListing : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyType PropertyType { get; set; }
    public decimal Price { get; set; }

    // Location
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal AreaSqFt { get; set; }

    /// <summary>Comma-free list of features, e.g. "Parking", "Garden".</summary>
    public List<string> Features { get; set; } = [];

    /// <summary>Identity user id of the owning broker.</summary>
    public string BrokerId { get; set; } = string.Empty;

    /// <summary>
    /// Estimated commission, computed from the DB commission rules when the listing is
    /// created or its price changes. Only shown to the owning broker.
    /// </summary>
    public decimal CommissionAmount { get; set; }
    public decimal CommissionRatePercent { get; set; }

    public List<PropertyImage> Images { get; set; } = [];
}
