namespace SSW.VerticalSliceArchitecture.Common.Pagination;

/// <summary>
/// The query-string contract shared by every paged list endpoint:
/// <c>?page=1&amp;pageSize=10&amp;sortBy=name&amp;sortDirection=asc</c>.
/// </summary>
/// <remarks>
/// Slice requests inherit this so the parameter names stay identical across features. Values are the
/// caller's raw input — run them through <see cref="PagingParams.From"/> and
/// <see cref="SortDirections.From"/> before building a query.
/// <para>
/// Every property is nullable, including the numbers: a non-nullable <c>int</c> with an initialiser looks
/// like a required parameter to Swagger, and the defaults belong in <see cref="PagingParams"/> anyway.
/// </para>
/// </remarks>
public abstract record PagedRequest
{
    public int? Page { get; init; }

    public int? PageSize { get; init; }

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }
}
