using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.Features.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Teams;

/// <summary>
/// The GraphQL shape of the <see cref="Team"/> aggregate.
/// </summary>
/// <remarks>
/// Both collections are resolved by a DataLoader rather than by the entity's navigation property.
/// Nothing eager-loads them, so the property would return the empty backing list and the client
/// would read that as "this team has no heroes".
/// </remarks>
[ObjectType<Team>]
public static partial class TeamType
{
    static partial void Configure(IObjectTypeDescriptor<Team> descriptor)
    {
        descriptor.Description("A team of heroes, and the missions they have taken on.");

        // An allow-list — see HeroType. Without it, ExecuteMission and CompleteCurrentMission
        // become mutations disguised as fields, and their ErrorOr result drags the ErrorOr types
        // into the schema.
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Id);
        descriptor.Field(t => t.Name);
        descriptor.Field(t => t.TotalPowerLevel);
        descriptor.Field(t => t.Status);
        descriptor.Field(t => t.CreatedAt);
        descriptor.Field(t => t.UpdatedAt);
    }

    public static async Task<IReadOnlyList<Hero>> GetHeroesAsync(
        [Parent] Team team,
        IHeroesByTeamIdDataLoader heroesByTeamId,
        CancellationToken cancellationToken)
        => await heroesByTeamId.LoadAsync(team.Id, cancellationToken) ?? [];

    public static async Task<IReadOnlyList<Mission>> GetMissionsAsync(
        [Parent] Team team,
        IMissionsByTeamIdDataLoader missionsByTeamId,
        CancellationToken cancellationToken)
        => await missionsByTeamId.LoadAsync(team.Id, cancellationToken) ?? [];
}
