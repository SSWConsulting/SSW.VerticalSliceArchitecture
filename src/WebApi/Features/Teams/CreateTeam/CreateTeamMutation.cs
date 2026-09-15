using SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.CreateTeam;

[MutationType]
public static partial class CreateTeamMutation
{
    /// <summary>Creates an empty team.</summary>
    [Error<InputValidationError>]
    public static async Task<Team> CreateTeamAsync(
        CreateTeamInput input,
        IValidator<CreateTeamInput> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);

        var team = Team.Create(input.Name);

        dbContext.Teams.Add(team);
        await dbContext.SaveChangesAsync(cancellationToken);

        return team;
    }
}
