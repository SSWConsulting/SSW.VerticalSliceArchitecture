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
    /// The query is returned unordered on purpose. The sorting middleware steps aside when the
    /// resolver already ordered the queryable, so an <c>OrderBy</c> here would silently make the
    /// <c>order</c> argument do nothing. Clients that page through the whole list should pass
    /// <c>order</c>, which is also what makes the cursors stable.
    /// </para>
    /// </remarks>
    [UsePaging]
    [UseFiltering<HeroFilterType>]
    [UseSorting<HeroSortType>]
    public static IQueryable<Hero> GetHeroes(ApplicationDbContext dbContext) => dbContext.Heroes;
}
