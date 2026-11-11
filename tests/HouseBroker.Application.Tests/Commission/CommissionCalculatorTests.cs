using FluentAssertions;
using FluentValidation;
using HouseBroker.Application.Commission;
using HouseBroker.Domain.Entities;
using Moq;

namespace HouseBroker.Application.Tests.Commission;

public class CommissionCalculatorTests
{
    // Same bands as the database seed: [0, 50L) 2 %, [50L, 1Cr] 1.75 %, (1Cr, ∞) 1.5 %.
    private static List<CommissionRule> SeedRules() =>
    [
        new() { MinPrice = 0m, MaxPrice = 5_000_000m, RatePercent = 2.00m },
        new() { MinPrice = 5_000_000m, MaxPrice = 10_000_000.01m, RatePercent = 1.75m },
        new() { MinPrice = 10_000_000.01m, MaxPrice = null, RatePercent = 1.50m }
    ];

    private static CommissionCalculator CreateSut(List<CommissionRule> rules)
    {
        var repo = new Mock<ICommissionRuleRepository>();
        repo.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(rules);
        return new CommissionCalculator(repo.Object);
    }

    [Theory]
    [InlineData("1000000", "2.00", "20000.00")]           // 10 lakh
    [InlineData("4999999", "2.00", "99999.98")]           // 49,99,999 — just under 50 lakh
    [InlineData("5000000", "1.75", "87500.00")]           // 50,00,000 — boundary goes to middle band
    [InlineData("7500000", "1.75", "131250.00")]          // 75 lakh
    [InlineData("10000000", "1.75", "175000.00")]         // 1,00,00,000 — exactly 1 crore stays 1.75 %
    [InlineData("10000000.01", "1.50", "150000.00")]      // first paisa above 1 crore
    [InlineData("10000001", "1.50", "150000.02")]         // 1,00,00,001 (rounded to 2 dp)
    [InlineData("25000000", "1.50", "375000.00")]         // 2.5 crore
    public async Task CalculateAsync_PriceInBand_UsesBandRate(string price, string expectedRate, string expectedAmount)
    {
        var sut = CreateSut(SeedRules());

        var result = await sut.CalculateAsync(decimal.Parse(price), CancellationToken.None);

        result.RatePercent.Should().Be(decimal.Parse(expectedRate));
        result.Amount.Should().Be(decimal.Parse(expectedAmount));
    }

    [Fact]
    public async Task CalculateAsync_InactiveRuleCoversPrice_IsIgnored()
    {
        var rules = SeedRules();
        rules.Insert(0, new CommissionRule { MinPrice = 0m, MaxPrice = null, RatePercent = 9m, IsActive = false });
        var sut = CreateSut(rules);

        var result = await sut.CalculateAsync(1_000_000m, CancellationToken.None);

        result.RatePercent.Should().Be(2.00m);
    }

    [Fact]
    public async Task CalculateAsync_RatesChangedInDatabase_UsesNewRate()
    {
        var rules = SeedRules();
        rules[0].RatePercent = 2.5m;
        var sut = CreateSut(rules);

        var result = await sut.CalculateAsync(1_000_000m, CancellationToken.None);

        result.Amount.Should().Be(25_000m);
    }

    [Fact]
    public async Task CalculateAsync_NoRuleCoversPrice_Throws()
    {
        var sut = CreateSut([new() { MinPrice = 0m, MaxPrice = 100m, RatePercent = 2m }]);

        var act = () => sut.CalculateAsync(1_000m, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CalculateAsync_NonPositivePrice_ThrowsValidationException(int price)
    {
        var sut = CreateSut(SeedRules());

        var act = () => sut.CalculateAsync(price, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetActiveRulesAsync_ReturnsActiveRulesLowestFirst()
    {
        var rules = SeedRules();
        rules.Reverse();
        rules.Add(new CommissionRule { MinPrice = 1m, RatePercent = 5m, IsActive = false });
        var sut = CreateSut(rules);

        var result = await sut.GetActiveRulesAsync(CancellationToken.None);

        result.Select(r => r.RatePercent).Should().Equal(2.00m, 1.75m, 1.50m);
    }
}
