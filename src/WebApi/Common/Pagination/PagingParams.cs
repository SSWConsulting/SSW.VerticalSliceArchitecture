namespace SSW.VerticalSliceArchitecture.Common.Pagination;

/// <summary>
/// A page request that has already been brought into range.
/// </summary>
/// <remarks>
/// The only place page and page size are clamped. Endpoints take a <see cref="PagingParams"/> rather
/// than two loose ints so a query can't be built from unbounded caller input.
/// </remarks>
public sealed record PagingParams
{
    public const int FirstPage = 1;
    public const int MinPageSize = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    private PagingParams(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; }

    public int PageSize { get; }

    public int Skip => (Page - FirstPage) * PageSize;

    /// <summary>
    /// Applies the defaults for anything the caller omitted, and brings the rest into range.
    /// </summary>
    /// <remarks>
    /// Clamped rather than rejected: the cap exists to stop a caller pulling the whole table, and
    /// failing the request would punish them for asking for more than they can have. An unknown sort
    /// column is treated differently — see <see cref="SortColumnMap{T}"/>.
    /// </remarks>
    public static PagingParams From(int? page, int? pageSize) =>
        new(Math.Max(page ?? FirstPage, FirstPage),
            Math.Clamp(pageSize ?? DefaultPageSize, MinPageSize, MaxPageSize));
}
