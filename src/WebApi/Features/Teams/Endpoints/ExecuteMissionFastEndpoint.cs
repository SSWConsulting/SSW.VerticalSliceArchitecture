using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record ExecuteMissionRequest(Guid TeamId, string Description);

public class ExecuteMissionFastEndpoint : Endpoint<ExecuteMissionRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public ExecuteMissionFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/{teamId}/execute-mission");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("ExecuteMissionFast")
            .Produces(404));
    }

    public override async Task HandleAsync(ExecuteMissionRequest req, CancellationToken ct)
    {
        var teamId = TeamId.From(req.TeamId);
        var team = _dbContext.Teams
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
            // DM: Figure out how to send errors
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await _dbContext.SaveChangesAsync(ct);

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