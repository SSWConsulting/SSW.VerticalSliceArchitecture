using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Commands;

public record ExecuteMissionRequest(string Description)
{
    public Guid TeamId { get; set; }
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

public class ExecuteMissionFastEndpoint : Endpoint<ExecuteMissionRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public ExecuteMissionFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/teams/{teamId}/execute-mission");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("ExecuteMissionFast")
            .WithTags("Teams")
            .Produces(200)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(ExecuteMissionRequest req, CancellationToken ct)
    {
        req.TeamId = Route<Guid>("teamId");

        var teamId = TeamId.From(req.TeamId);
        var team = _dbContext.Teams
            .WithSpecification(new TeamByIdSpec(teamId))
            .FirstOrDefault();

        if (team is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = new[] { new { TeamErrors.NotFound.Code, TeamErrors.NotFound.Description } }
            }, ct);
            return;
        }

        var result = team.ExecuteMission(req.Description);
        if (result.IsError)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = result.Errors.Select(e => new { e.Code, e.Description })
            }, ct);
            return;
        }

        await _dbContext.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = StatusCodes.Status200OK;
    }
}
