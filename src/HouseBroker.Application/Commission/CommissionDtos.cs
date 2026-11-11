namespace HouseBroker.Application.Commission;

/// <summary>Commission estimate for one price.</summary>
public sealed record CommissionResult(decimal Price, decimal RatePercent, decimal Amount);

/// <summary>One commission band as configured in the database.</summary>
public sealed record CommissionRuleDto(Guid Id, decimal MinPrice, decimal? MaxPrice, decimal RatePercent);
