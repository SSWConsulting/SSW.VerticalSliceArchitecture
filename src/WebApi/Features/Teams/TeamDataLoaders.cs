using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams;

public static class TeamDataLoaders
{
    /// <remarks>
    /// A batch DataLoader — one team per key. Used by the team field on Hero, where a page of
    /// heroes would otherwise be one query per hero.
    /// </remarks>
    [DataLoader]
    public static async Task<Dictionary<TeamId, Team>> GetTeamByIdAsync(
        IReadOnlyList<TeamId> teamIds,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Teams
            .Where(t => teamIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, cancellationToken);

    /// <remarks>
    /// Queries through the team rather than a mission set: the foreign key to the team is a shadow
    /// property, so <c>Mission</c> has nothing to filter on.
    /// </remarks>
    [DataLoader]
    public static async Task<Dictionary<TeamId, Mission[]>> GetMissionsByTeamIdAsync(
        IReadOnlyList<TeamId> teamIds,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Teams
            .Where(t => teamIds.Contains(t.Id))
            .Select(t => new { t.Id, Missions = t.Missions.ToArray() })
            .ToDictionaryAsync(x => x.Id, x => x.Missions, cancellationToken);
}
