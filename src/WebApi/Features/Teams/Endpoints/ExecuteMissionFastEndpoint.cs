using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record ExecuteMissionRequest(Guid TeamId, string Description);

public class ExecuteMissionFastEndpoint(ApplicationDbContext dbContext) 
    : Endpoint<ExecuteMissionRequest>
{
    public override void Configure()
    {
        Post("/{teamId}/execute-mission");
        Group<TeamsGroup>();
        Description(x => x.WithName("ExecuteMissionFast"));
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

public class ExecuteMissionRequestValidator : Validator<ExecuteMissionRequest>
{
    public ExecuteMissionRequestValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();

        RuleFor(v => v.Description)
            .NotEmpty();
    }
}

public class ExecuteMissionSummary : Summary<ExecuteMissionFastEndpoint>
{
    public ExecuteMissionSummary()
    {
        Summary = "Execute a new mission";
        Description = "Assigns a new mission to the team. The team must not have an active mission.";
        
        ExampleRequest = new ExecuteMissionRequest(
            TeamId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Description: "Stop the alien invasion in New York City");
    }
}