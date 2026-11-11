using HouseBroker.Application.Common.Pagination;
using Microsoft.EntityFrameworkCore;

namespace HouseBroker.Infrastructure.Persistence;

/// <summary>EF Core helpers shared by repositories.</summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Runs a COUNT and a single page query (OFFSET/FETCH) and wraps them in a <see cref="PagedResult{T}"/>.
    /// The query must already be ordered so pages are stable.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, PageRequest paging, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip(paging.Skip).Take(paging.PageSize).ToListAsync(ct);
        return new PagedResult<T>(items, paging.Page, paging.PageSize, total);
    }
}
