using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.ExecuteMission;

[MutationType]
public static partial class ExecuteMissionMutation
{
    /// <summary>Sends an available team on a new mission.</summary>
    /// <remarks>
    /// <c>ThrowOnError</c> turns the domain's <c>ErrorOr</c> result into the exception the mutation
    /// conventions put in the payload — here a <c>ConflictError</c> when the team is already out or
    /// has no heroes.
    /// </remarks>
    [Error<InputValidationError>]
    [Error<NotFoundError>]
    [Error<ConflictError>]
    public static async Task<Team> ExecuteMissionAsync(
        ExecuteMissionInput input,
        IValidator<ExecuteMissionInput> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);

        var team = await dbContext.Teams
                       .WithSpecification(TeamSpec.ById(input.TeamId))
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new NotFoundException(TeamErrors.NotFound);

        team.ExecuteMission(input.Description).ThrowOnError();

        await dbContext.SaveChangesAsync(cancellationToken);

        return team;
    }
}
