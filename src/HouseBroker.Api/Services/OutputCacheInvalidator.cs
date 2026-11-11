using HouseBroker.Application.Common.Interfaces;
using Microsoft.AspNetCore.OutputCaching;

namespace HouseBroker.Api.Services;

/// <summary>
/// Evicts ASP.NET Core output-cache entries by tag after listings change.
/// Eviction runs after the database commit, so a failure here is logged rather than thrown:
/// the write succeeded, and cached entries expire on their own within the policy's 60 s.
/// </summary>
public sealed class OutputCacheInvalidator(IOutputCacheStore store, ILogger<OutputCacheInvalidator> logger) : ICacheInvalidator
{
    public async Task EvictByTagAsync(string tag, CancellationToken ct)
    {
        try
        {
            await store.EvictByTagAsync(tag, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Output cache eviction failed for tag {Tag}", tag);
        }
    }
}
