using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.GetAllTeams;

[QueryType]
public static partial class GetAllTeamsQuery
{
    /// <summary>A page of teams, as a Relay connection.</summary>
    [UsePaging]
    [UseFiltering<TeamFilterType>]
    [UseSorting<TeamSortType>]
    public static IQueryable<Team> GetTeams(ApplicationDbContext dbContext) =>
        dbContext.Teams.OrderBy(t => t.Name);
}
