using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.GetAllHeroes;

[QueryType]
public static partial class GetAllHeroesQuery
{
    /// <summary>A page of heroes, as a Relay connection.</summary>
    /// <remarks>
    /// The middleware order is fixed: paging runs last, so filtering and sorting shape the query
    /// before it is sliced. Reversing the attributes pages the whole table and then filters the page.
    /// <para>
    /// The <c>OrderBy</c> is the default the connection falls back to. Cursor paging needs a stable
    /// order, and a query with no order clause has none.
    /// </para>
    /// </remarks>
    [UsePaging]
    [UseFiltering<HeroFilterType>]
    [UseSorting<HeroSortType>]
    public static IQueryable<Hero> GetHeroes(ApplicationDbContext dbContext) =>
        dbContext.Heroes.OrderBy(h => h.Name);
}
