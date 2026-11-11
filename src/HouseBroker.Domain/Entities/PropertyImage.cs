using HouseBroker.Domain.Common;

namespace HouseBroker.Domain.Entities;

public class PropertyImage : BaseEntity
{
    public Guid PropertyListingId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
