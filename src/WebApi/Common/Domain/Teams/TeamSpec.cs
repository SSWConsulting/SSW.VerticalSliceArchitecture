using System.Linq.Expressions;
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Teams;

// For more on the Specification Pattern see: https://www.ssw.com.au/rules/use-specification-pattern/
public sealed class TeamSpec : Specification<Team>
{
    /// <summary>
    /// The columns a caller may sort teams by. Lives with the aggregate because mapping a query-string
    /// name to an ordering expression is knowledge of the entity, not of any one slice.
    /// </summary>
    public static SortColumnMap<Team> SortColumns { get; } = new(
        defaultColumn: "name",
        columns: new Dictionary<string, Expression<Func<Team, object?>>>
        {
            ["name"] = t => t.Name,
            ["totalPowerLevel"] = t => t.TotalPowerLevel
        });

    public static TeamSpec ById(TeamId teamId)
    {
        var spec = new TeamSpec();
        spec.Query
            .Where(t => t.Id == teamId)
            .Include(t => t.Missions)
            .Include(t => t.Heroes);
        return spec;
    }

    /// <remarks>
    /// No <c>Include</c>, unlike <see cref="ById"/>: the list endpoint projects into a DTO, and EF Core
    /// discards includes under a projection — keeping them would cost a join per page and buy nothing.
    /// </remarks>
    public static TeamSpec Paged(PagingParams paging, string? sortBy, SortDirection sortDirection)
    {
        ThrowIfNull(paging);

        var spec = new TeamSpec();
        SortColumns.Apply(spec.Query, sortBy, sortDirection, t => t.Id);
        spec.Query.Skip(paging.Skip).Take(paging.PageSize);
        return spec;
    }
}
