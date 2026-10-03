using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.Features.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Heroes;

/// <summary>
/// The GraphQL shape of the <see cref="Hero"/> aggregate.
/// </summary>
/// <remarks>
/// The binding lives here rather than as attributes on the entity, so the domain stays free of
/// GraphQL. It sits at the feature level, not in a slice: every hero slice returns this one type,
/// which is the point of a graph — one Hero, whichever field led the client to it.
/// </remarks>
[ObjectType<Hero>]
public static partial class HeroType
{
    static partial void Configure(IObjectTypeDescriptor<Hero> descriptor)
    {
        descriptor.Description("A hero, with the powers that add up to their power level.");

        // An allow-list, because the default binds every public member of the entity — including
        // its behaviour. UpdatePowers and PopDomainEvents would become schema fields, and a
        // domain method added later would silently join the public API.
        descriptor.BindFieldsExplicitly();

        descriptor.Field(h => h.Id);
        descriptor.Field(h => h.Name);
        descriptor.Field(h => h.Alias);
        descriptor.Field(h => h.PowerLevel);
        descriptor.Field(h => h.Powers);
        descriptor.Field(h => h.CreatedAt);
        descriptor.Field(h => h.UpdatedAt);
    }

    /// <summary>The team this hero belongs to, or null when they are unassigned.</summary>
    public static async Task<Team?> GetTeamAsync(
        [Parent] Hero hero,
        ITeamByIdDataLoader teamById,
        CancellationToken cancellationToken)
        => hero.TeamId is null ? null : await teamById.LoadAsync(hero.TeamId.Value, cancellationToken);
}
