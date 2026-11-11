namespace HouseBroker.Application.Tests.TestDoubles;

/// <summary>A clock that always returns the same instant, so timestamps can be asserted.</summary>
public sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
}
