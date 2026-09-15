using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Heroes;

/// <summary>
/// Batched loads of heroes, so that a field on a list of parents costs one query rather than one
/// query per parent.
/// </summary>
/// <remarks>
/// The source generator turns each method into a DataLoader class and an interface named after the
/// method with the <c>Get</c> prefix and <c>Async</c> suffix removed:
/// <c>GetHeroesByTeamIdAsync</c> becomes <c>IHeroesByTeamIdDataLoader</c>.
/// </remarks>
public static class HeroDataLoaders
{
    /// <remarks>
    /// A group DataLoader — the value is an array, because a team has many heroes. The generated
    /// loader returns null for a key with no heroes, so callers coalesce to an empty list.
    /// </remarks>
    [DataLoader]
    public static async Task<Dictionary<TeamId, Hero[]>> GetHeroesByTeamIdAsync(
        IReadOnlyList<TeamId> teamIds,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.Heroes
            .Where(h => h.TeamId != null && teamIds.Contains(h.TeamId.Value))
            .GroupBy(h => h.TeamId!.Value)
            .Select(g => new { g.Key, Heroes = g.OrderBy(h => h.Name).ToArray() })
            .ToDictionaryAsync(g => g.Key, g => g.Heroes, cancellationToken);
}
