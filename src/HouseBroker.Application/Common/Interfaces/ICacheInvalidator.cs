namespace HouseBroker.Application.Common.Interfaces;

/// <summary>
/// Evicts cached HTTP responses. Responses are cached by ASP.NET Core output caching in
/// the Api layer; services call this after a write so readers never see stale data.
/// </summary>
public interface ICacheInvalidator
{
    Task EvictByTagAsync(string tag, CancellationToken ct);
}
