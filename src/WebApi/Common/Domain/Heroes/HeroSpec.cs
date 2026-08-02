using System.Linq.Expressions;
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

// For more on the Specification Pattern see: https://www.ssw.com.au/rules/use-specification-pattern/
public sealed class HeroSpec : Specification<Hero>
{
    /// <summary>
    /// The columns a caller may sort heroes by. Lives with the aggregate because mapping a query-string
    /// name to an ordering expression is knowledge of the entity, not of any one slice.
    /// </summary>
    public static SortColumnMap<Hero> SortColumns { get; } = new(
        defaultColumn: "name",
        columns: new Dictionary<string, Expression<Func<Hero, object?>>>
        {
            ["name"] = h => h.Name,
            ["alias"] = h => h.Alias,
            ["powerLevel"] = h => h.PowerLevel
        });

    public static HeroSpec ById(HeroId heroId)
    {
        var spec = new HeroSpec();
        spec.Query.Where(h => h.Id == heroId);
        return spec;
    }

    public static HeroSpec Paged(PagingParams paging, string? sortBy, SortDirection sortDirection)
    {
        ThrowIfNull(paging);

        var spec = new HeroSpec();
        SortColumns.Apply(spec.Query, sortBy, sortDirection, h => h.Id);
        spec.Query.Skip(paging.Skip).Take(paging.PageSize);
        return spec;
    }
}
