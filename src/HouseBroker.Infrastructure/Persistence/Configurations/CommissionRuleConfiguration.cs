using HouseBroker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HouseBroker.Infrastructure.Persistence.Configurations;

/// <summary>
/// Commission bands. Seeded with the assignment's rates (INR); change the rows in the
/// database to change the rates.
/// </summary>
public sealed class CommissionRuleConfiguration : IEntityTypeConfiguration<CommissionRule>
{
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<CommissionRule> b)
    {
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.MinPrice).HasPrecision(18, 2);
        b.Property(x => x.MaxPrice).HasPrecision(18, 2);
        b.Property(x => x.RatePercent).HasPrecision(5, 3);
        b.ToTable(t => t.HasCheckConstraint("CK_CommissionRule_Rate", "[RatePercent] >= 0 AND [RatePercent] <= 100"));

        // Bands are [MinPrice, MaxPrice). Prices have 2 decimals, so 1,00,00,000.01 as the
        // boundary keeps exactly 1 crore in the 1.75 % band ("between 50 lakh and 1 crore")
        // and anything above 1 crore in the 1.5 % band.
        b.HasData(
            new CommissionRule { Id = Guid.Parse("8a1f0000-0000-0000-0000-000000000001"), MinPrice = 0m, MaxPrice = 5_000_000m, RatePercent = 2.00m, CreatedAtUtc = SeedDate },
            new CommissionRule { Id = Guid.Parse("8a1f0000-0000-0000-0000-000000000002"), MinPrice = 5_000_000m, MaxPrice = 10_000_000.01m, RatePercent = 1.75m, CreatedAtUtc = SeedDate },
            new CommissionRule { Id = Guid.Parse("8a1f0000-0000-0000-0000-000000000003"), MinPrice = 10_000_000.01m, MaxPrice = null, RatePercent = 1.50m, CreatedAtUtc = SeedDate });
    }
}
