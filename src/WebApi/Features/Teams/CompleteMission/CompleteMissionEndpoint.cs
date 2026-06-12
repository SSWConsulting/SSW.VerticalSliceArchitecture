using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.CompleteMission;

public class CompleteMissionEndpoint(ApplicationDbContext dbContext)
    : Endpoint<CompleteMissionRequest>
{
    public override void Configure()
    {
        Post("/{teamId}/complete-mission");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("CompleteMission")
            .Produces(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(CompleteMissionRequest req, CancellationToken ct)
    {
        var teamId = TeamId.From(req.TeamId);
        var team = dbContext.Teams
            .WithSpecification(TeamSpec.ById(teamId))
            .FirstOrDefault();

        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var result = team.CompleteCurrentMission();
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
