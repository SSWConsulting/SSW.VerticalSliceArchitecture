using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;
using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.AddHeroToTeam;

[MutationType]
public static partial class AddHeroToTeamMutation
{
    /// <summary>Adds an existing hero to an existing team.</summary>
    [Error<InputValidationError>]
    [Error<NotFoundError>]
    public static async Task<Team> AddHeroToTeamAsync(
        AddHeroToTeamInput input,
        IValidator<AddHeroToTeamInput> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);

        var team = await dbContext.Teams
                       .WithSpecification(TeamSpec.ById(input.TeamId))
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new NotFoundException(TeamErrors.NotFound);

        var hero = await dbContext.Heroes
                       .WithSpecification(HeroSpec.ById(input.HeroId))
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new NotFoundException(HeroErrors.NotFound);

        team.AddHero(hero);
        await dbContext.SaveChangesAsync(cancellationToken);

        return team;
    }
}
