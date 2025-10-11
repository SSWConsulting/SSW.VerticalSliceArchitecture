using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record CompleteMissionRequest
{
    public Guid TeamId { get; set; }
}

public class CompleteMissionRequestValidator : Validator<CompleteMissionRequest>
{
    public CompleteMissionRequestValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();
    }
}

public class CompleteMissionFastEndpoint : Endpoint<CompleteMissionRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public CompleteMissionFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/teams/{teamId}/complete-mission");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("CompleteMissionFast")
            .WithTags("Teams")
            .Produces(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(CompleteMissionRequest req, CancellationToken ct)
    {
        req.TeamId = Route<Guid>("teamId");

        var teamId = TeamId.From(req.TeamId);
        var team = _dbContext.Teams
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
            // DM: Figure out how to send errors
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await _dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}