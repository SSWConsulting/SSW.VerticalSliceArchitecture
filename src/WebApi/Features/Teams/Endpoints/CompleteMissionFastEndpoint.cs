using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using IEndpoint = FastEndpoints.IEndpoint;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record CompleteMissionRequest(Guid TeamId);

public class CompleteMissionFastEndpoint(ApplicationDbContext dbContext) 
    : Endpoint<CompleteMissionRequest>
{
    public override void Configure()
    {
        Post("/{teamId}/complete-mission");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("CompleteMissionFast")
            .Produces(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(CompleteMissionRequest req, CancellationToken ct)
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

public class CompleteMissionRequestValidator : Validator<CompleteMissionRequest>
{
    public CompleteMissionRequestValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();
    }
}

public class CompleteMissionSummary : Summary<CompleteMissionFastEndpoint>
{
    public CompleteMissionSummary()
    {
        Summary = "Complete the current mission";
        Description = "Marks the team's current mission as completed. The team must have an active mission.";
    }
}