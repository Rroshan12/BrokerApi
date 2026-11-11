using HouseBroker.Application.Commission;
using HouseBroker.Domain.Entities;
using HouseBroker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HouseBroker.Infrastructure.Repositories;

/// <summary>EF Core access to the CommissionRules table.</summary>
public sealed class CommissionRuleRepository(AppDbContext db) : ICommissionRuleRepository
{
    public async Task<IReadOnlyList<CommissionRule>> GetActiveAsync(CancellationToken ct) =>
        await db.CommissionRules
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.MinPrice)
            .ToListAsync(ct);
}
