using FluentValidation;

namespace HouseBroker.Application.Commission;

/// <summary>
/// Commission engine. Rates are never hard-coded: they come from the CommissionRules table,
/// so a band or rate can be changed in the database without a deploy.
/// </summary>
public sealed class CommissionCalculator(ICommissionRuleRepository rules) : ICommissionCalculator
{
    /// <inheritdoc />
    public async Task<CommissionResult> CalculateAsync(decimal price, CancellationToken ct)
    {
        if (price <= 0)
            throw new ValidationException("Price must be greater than zero.");

        var activeRules = await rules.GetActiveAsync(ct);
        var rule = activeRules.OrderBy(r => r.MinPrice).FirstOrDefault(r => r.Matches(price))
            ?? throw new InvalidOperationException($"No active commission rule covers a price of {price}.");

        return new CommissionResult(price, rule.RatePercent, rule.CalculateCommission(price));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CommissionRuleDto>> GetActiveRulesAsync(CancellationToken ct)
    {
        var activeRules = await rules.GetActiveAsync(ct);
        return activeRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.MinPrice)
            .Select(r => new CommissionRuleDto(r.Id, r.MinPrice, r.MaxPrice, r.RatePercent))
            .ToList();
    }
}
