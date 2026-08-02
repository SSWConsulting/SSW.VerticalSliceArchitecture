namespace SSW.VerticalSliceArchitecture.Common.Pagination;

/// <summary>
/// The response envelope every paged list endpoint returns.
/// </summary>
/// <remarks>
/// One shape for every list endpoint, so a generated client can treat paging uniformly instead of
/// learning a new envelope per feature.
/// </remarks>
public sealed record PagedList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}
