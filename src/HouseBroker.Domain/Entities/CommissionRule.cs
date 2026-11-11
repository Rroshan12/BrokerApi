using HouseBroker.Domain.Common;

namespace HouseBroker.Domain.Entities;

/// <summary>
/// A commission band stored in the database so rates can change without a deploy.
/// A rule applies when MinPrice &lt;= price &lt; MaxPrice (MaxPrice null = no upper bound).
/// </summary>
public class CommissionRule : BaseEntity
{
    public decimal MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal RatePercent { get; set; }
    public bool IsActive { get; set; } = true;

    public bool Matches(decimal price) =>
        IsActive && price >= MinPrice && (MaxPrice is null || price < MaxPrice);
}
