namespace SSW.VerticalSliceArchitecture.Common.Pagination;

/// <summary>
/// The <c>sortDirection</c> query values callers may send, and their <see cref="SortDirection"/> meaning.
/// </summary>
/// <remarks>
/// The direction arrives as a string rather than binding straight to the enum so an unrecognised value
/// fails validation with a message listing what is allowed, instead of a model-binding error.
/// </remarks>
public static class SortDirections
{
    private static readonly Dictionary<string, SortDirection> Directions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["asc"] = SortDirection.Ascending,
        ["ascending"] = SortDirection.Ascending,
        ["desc"] = SortDirection.Descending,
        ["descending"] = SortDirection.Descending
    };

    public static IReadOnlyCollection<string> Allowed => Directions.Keys;

    /// <summary>
    /// Whether <paramref name="sortDirection"/> is a direction this API accepts. Blank means "unspecified",
    /// which is allowed and resolves to ascending.
    /// </summary>
    public static bool IsAllowed(string? sortDirection) =>
        string.IsNullOrWhiteSpace(sortDirection) || Directions.ContainsKey(sortDirection);

    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sortDirection"/> is not one of <see cref="Allowed"/>. Unreachable through an
    /// endpoint, where the request validator rejects it first; a throw here means validation was bypassed.
    /// </exception>
    public static SortDirection From(string? sortDirection)
    {
        if (string.IsNullOrWhiteSpace(sortDirection))
            return SortDirection.Ascending;

        if (!Directions.TryGetValue(sortDirection, out var direction))
            throw new ArgumentOutOfRangeException(
                nameof(sortDirection),
                sortDirection,
                $"Expected one of: {string.Join(", ", Allowed)}.");

        return direction;
    }
}
