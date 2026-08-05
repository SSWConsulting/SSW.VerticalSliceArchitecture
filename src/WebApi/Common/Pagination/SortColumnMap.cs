using System.Linq.Expressions;

namespace SSW.VerticalSliceArchitecture.Common.Pagination;

/// <summary>
/// The columns callers may sort an aggregate by, mapped to the expressions that order it.
/// </summary>
/// <remarks>
/// A caller's <c>sortBy</c> is looked up in this map and never reaches a query as text, so an unknown
/// column can't become SQL. Unlike page size, an unknown column is rejected rather than ignored — a
/// silent fallback returns a plausible-looking page in the wrong order and hides the caller's bug.
/// </remarks>
public sealed class SortColumnMap<T>
{
    private readonly Dictionary<string, Expression<Func<T, object?>>> _columns;
    private readonly string _defaultColumn;

    public SortColumnMap(string defaultColumn, IDictionary<string, Expression<Func<T, object?>>> columns)
    {
        ThrowIfNull(columns);
        ThrowIfNullOrWhiteSpace(defaultColumn);

        _columns = new Dictionary<string, Expression<Func<T, object?>>>(columns, StringComparer.OrdinalIgnoreCase);

        if (!_columns.ContainsKey(defaultColumn))
            throw new ArgumentException(
                $"Default sort column '{defaultColumn}' is not one of: {string.Join(", ", _columns.Keys)}.",
                nameof(defaultColumn));

        _defaultColumn = defaultColumn;
    }

    public IReadOnlyCollection<string> AllowedColumns => _columns.Keys;

    /// <summary>
    /// Whether <paramref name="sortBy"/> is a sortable column. Blank means "unspecified", which is
    /// allowed and resolves to the default column.
    /// </summary>
    public bool IsAllowed(string? sortBy) =>
        string.IsNullOrWhiteSpace(sortBy) || _columns.ContainsKey(sortBy);

    /// <summary>
    /// Orders a specification by the requested column, then by <paramref name="tieBreaker"/>.
    /// </summary>
    /// <remarks>
    /// The tie-breaker is what makes paging stable. <c>OFFSET</c>/<c>FETCH</c> gives no defined order to
    /// rows that tie on the sort column, so without it the same row can appear on two pages, or on none.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sortBy"/> is not one of <see cref="AllowedColumns"/>. Unreachable through an
    /// endpoint, where the request validator rejects it first; a throw here means validation was bypassed.
    /// </exception>
    public IOrderedSpecificationBuilder<T> Apply(
        ISpecificationBuilder<T> query,
        string? sortBy,
        SortDirection direction,
        Expression<Func<T, object?>> tieBreaker)
    {
        ThrowIfNull(query);
        ThrowIfNull(tieBreaker);

        var column = Resolve(sortBy);

        var ordered = direction == SortDirection.Descending
            ? query.OrderByDescending(column)
            : query.OrderBy(column);

        return ordered.ThenBy(tieBreaker);
    }

    private Expression<Func<T, object?>> Resolve(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return _columns[_defaultColumn];

        if (!_columns.TryGetValue(sortBy, out var column))
            throw new ArgumentOutOfRangeException(
                nameof(sortBy),
                sortBy,
                $"Expected one of: {string.Join(", ", AllowedColumns)}.");

        return column;
    }
}
