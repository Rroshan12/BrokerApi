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

    /// <summary>Rate as a percentage, e.g. 1.75 means 1.75 %.</summary>
    public decimal RatePercent { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>True when this rule is active and the price falls inside its band.</summary>
    public bool Matches(decimal price) =>
        IsActive && price >= MinPrice && (MaxPrice is null || price < MaxPrice);

    /// <summary>Commission for a price at this rule's rate, rounded to 2 decimals.</summary>
    public decimal CalculateCommission(decimal price) =>
        Math.Round(price * RatePercent / 100m, 2, MidpointRounding.AwayFromZero);
}
