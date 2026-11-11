namespace HouseBroker.Application.Commission;

/// <summary>Commission engine: picks the DB-configured band for a price and calculates the amount.</summary>
public interface ICommissionCalculator
{
    /// <summary>Estimated broker commission for the given listing price.</summary>
    Task<CommissionResult> CalculateAsync(decimal price, CancellationToken ct);

    /// <summary>The active commission bands, lowest first.</summary>
    Task<IReadOnlyList<CommissionRuleDto>> GetActiveRulesAsync(CancellationToken ct);
}
