using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Queries;

public record GetTeamTeamDto(Guid Id, string Name, IEnumerable<GetTeamHeroDto> Heroes);

public record GetTeamHeroDto(Guid Id, string Name, string Alias, int PowerLevel, IEnumerable<GetTeamHeroPowerDto> Powers);

public record GetTeamHeroPowerDto(string Name, int PowerLevel);

public record GetTeamRequest
{
    public Guid TeamId { get; set; }
}

public class GetTeamRequestValidator : Validator<GetTeamRequest>
{
    public GetTeamRequestValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();
    }
}

public class GetTeamFastEndpoint : Endpoint<GetTeamRequest, GetTeamTeamDto>
{
    private readonly ApplicationDbContext _dbContext;

    public GetTeamFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/teams/{teamId}");
        Group<TeamsGroup>();
        AllowAnonymous();
        Description(x => x
            .WithName("GetTeamFast")
            .WithTags("Teams")
            .Produces<GetTeamTeamDto>(200)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(GetTeamRequest req, CancellationToken ct)
    {
        req.TeamId = Route<Guid>("teamId");

        var teamId = TeamId.From(req.TeamId);

        var team = await _dbContext.Teams
            .Where(t => t.Id == teamId)
            .Select(t => new GetTeamTeamDto(
                t.Id.Value,
                t.Name,
                t.Heroes.Select(
                    h => new GetTeamHeroDto(
                        h.Id.Value, 
                        h.Name, 
                        h.Alias, 
                        h.PowerLevel, 
                        h.Powers.Select(p => new GetTeamHeroPowerDto(p.Name, p.PowerLevel))
                    )
                )))
            .FirstOrDefaultAsync(ct);

        if (team is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = new[] { new { TeamErrors.NotFound.Code, TeamErrors.NotFound.Description } }
            }, ct);
            return;
        }

        await HttpContext.Response.WriteAsJsonAsync(team, ct);
    }
}
