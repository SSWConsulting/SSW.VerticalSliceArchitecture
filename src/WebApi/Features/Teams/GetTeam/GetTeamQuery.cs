using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.GetTeam;

[QueryType]
public static partial class GetTeamQuery
{
    /// <summary>One team by id, or null when no team has that id.</summary>
    /// <remarks>
    /// No <c>Include</c>: the heroes and missions fields are resolved by their own DataLoaders, and
    /// only when the client asks for them.
    /// </remarks>
    public static async Task<Team?> GetTeamByIdAsync(
        TeamId teamId,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);
}
