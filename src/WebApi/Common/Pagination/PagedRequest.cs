namespace SSW.VerticalSliceArchitecture.Common.Pagination;

/// <summary>
/// The query-string contract shared by every paged list endpoint:
/// <c>?page=1&amp;pageSize=10&amp;sortBy=name&amp;sortDirection=asc</c>.
/// </summary>
/// <remarks>
/// Slice requests inherit this so the parameter names stay identical across features. Values are the
/// caller's raw input — run them through <see cref="PagingParams.From"/> and
/// <see cref="SortDirections.From"/> before building a query.
/// </remarks>
public abstract record PagedRequest
{
    public int Page { get; init; } = PagingParams.FirstPage;

    public int PageSize { get; init; } = PagingParams.DefaultPageSize;

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }
}
