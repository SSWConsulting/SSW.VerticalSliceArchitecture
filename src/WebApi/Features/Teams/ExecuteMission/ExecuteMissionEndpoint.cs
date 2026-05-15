using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.ExecuteMission;

public class ExecuteMissionEndpoint(ApplicationDbContext dbContext)
    : Endpoint<ExecuteMissionRequest>
{
    public override void Configure()
    {
        Post("/{teamId}/execute-mission");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("ExecuteMission")
            .Produces(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(ExecuteMissionRequest req, CancellationToken ct)
    {
        var teamId = TeamId.From(req.TeamId);
        var team = dbContext.Teams
            .WithSpecification(new TeamByIdSpec(teamId))
            .FirstOrDefault();

        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = team.ExecuteMission(req.Description);
        if (result.IsError)
        {
            result.Errors.ForEach(e => AddError(e.Description, e.Code));
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
