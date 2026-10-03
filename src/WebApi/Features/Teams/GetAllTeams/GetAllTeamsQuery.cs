using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.GetAllTeams;

[QueryType]
public static partial class GetAllTeamsQuery
{
    /// <summary>A page of teams, as a Relay connection.</summary>
    /// <remarks>
    /// Unordered on purpose — see <see cref="Heroes.GetAllHeroes.GetAllHeroesQuery"/> for why an
    /// <c>OrderBy</c> here would disable the <c>order</c> argument.
    /// </remarks>
    [UsePaging]
    [UseFiltering<TeamFilterType>]
    [UseSorting<TeamSortType>]
    public static IQueryable<Team> GetTeams(ApplicationDbContext dbContext) => dbContext.Teams;
}
