using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.CompleteMission;

[MutationType]
public static partial class CompleteMissionMutation
{
    /// <summary>Marks the team's current mission complete.</summary>
    [Error<InputValidationError>]
    [Error<NotFoundError>]
    [Error<ConflictError>]
    public static async Task<Team> CompleteMissionAsync(
        CompleteMissionInput input,
        IValidator<CompleteMissionInput> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);

        var team = await dbContext.Teams
                       .WithSpecification(TeamSpec.ById(input.TeamId))
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new NotFoundException(TeamErrors.NotFound);

        team.CompleteCurrentMission().ThrowOnError();

        await dbContext.SaveChangesAsync(cancellationToken);

        return team;
    }
}
