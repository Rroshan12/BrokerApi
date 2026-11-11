using HouseBroker.Domain.Entities;

namespace HouseBroker.Application.Commission;

/// <summary>Reads commission bands from the CommissionRules table.</summary>
public interface ICommissionRuleRepository
{
    /// <summary>Active rules ordered by MinPrice.</summary>
    Task<IReadOnlyList<CommissionRule>> GetActiveAsync(CancellationToken ct);
}
