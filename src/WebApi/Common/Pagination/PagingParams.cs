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

    /// <remarks>
    /// Computed in <c>long</c> and capped: <c>(Page - 1) * PageSize</c> overflows <c>int</c> well before
    /// <c>Page</c> does, and an overflowed offset wraps to a negative or arbitrary positive value that EF
    /// hands to SQL Server as the <c>OFFSET</c>. Saturating at <see cref="int.MaxValue"/> keeps a
    /// far-past-the-end page doing what every other past-the-end page does — return no rows.
    /// </remarks>
    public int Skip => (int)Math.Min((long)(Page - FirstPage) * PageSize, int.MaxValue);

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

    /// <summary>
    /// Recovers the page window a specification was built with.
    /// </summary>
    /// <remarks>
    /// Lets the response envelope be derived from the query that actually ran, so the two can't disagree.
    /// Note this reverses <see cref="Skip"/>, which saturates: a page far enough past the end reports the
    /// page that <c>Skip</c> landed on rather than the one asked for. That only bites past
    /// <c>int.MaxValue</c> rows of offset, where every page is empty anyway.
    /// </remarks>
    /// <exception cref="ArgumentException">The specification carries no <c>Skip</c>/<c>Take</c>.</exception>
    public static PagingParams FromSpecification<T>(ISpecification<T> specification)
    {
        ThrowIfNull(specification);

        // Ardalis leaves Skip/Take at a non-positive sentinel until a builder sets them, so a Take that
        // isn't a real page size means the spec never went through a Paged factory.
        if (specification.Take < MinPageSize || specification.Skip < 0)
            throw new ArgumentException(
                "Specification has no page window. Build it with an {Aggregate}Spec.Paged(...) factory.",
                nameof(specification));

        return new PagingParams((specification.Skip / specification.Take) + FirstPage, specification.Take);
    }
}
