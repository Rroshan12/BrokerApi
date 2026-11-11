namespace HouseBroker.Application.Common.Pagination;

/// <summary>
/// Paging parameters shared by every list endpoint. Bound from the query string
/// (<c>?page=2&amp;pageSize=20</c>); queries with filters inherit from it.
/// </summary>
public record PageRequest
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    /// <summary>1-based page number.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Items per page, 1 to 50.</summary>
    public int PageSize { get; init; } = DefaultPageSize;

    /// <summary>Number of items to skip for this page. Not bound from the request.</summary>
    public int Skip => (Page - 1) * PageSize;
}
