using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Commands;

public record AddHeroToTeamRequest
{
    public Guid TeamId { get; set; }
    public Guid HeroId { get; set; }
}

public class AddHeroToTeamRequestValidator : Validator<AddHeroToTeamRequest>
{
    public AddHeroToTeamRequestValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();

        RuleFor(v => v.HeroId)
            .NotEmpty();
    }
}

public class AddHeroToTeamFastEndpoint : Endpoint<AddHeroToTeamRequest>
{
    private readonly ApplicationDbContext _dbContext;

    public AddHeroToTeamFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/teams/{teamId}/heroes/{heroId}");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("AddHeroToTeamFast")
            .WithTags("Teams")
            .Produces(201)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(404)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(AddHeroToTeamRequest req, CancellationToken ct)
    {
        req.TeamId = Route<Guid>("teamId");
        req.HeroId = Route<Guid>("heroId");

        var teamId = TeamId.From(req.TeamId);
        var heroId = HeroId.From(req.HeroId);

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

        var hero = _dbContext.Heroes
            .WithSpecification(new HeroByIdSpec(heroId))
            .FirstOrDefault();

        if (hero is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await HttpContext.Response.WriteAsJsonAsync(new
            {
                errors = new[] { new { HeroErrors.NotFound.Code, HeroErrors.NotFound.Description } }
            }, ct);
            return;
        }

        team.AddHero(hero);
        await _dbContext.SaveChangesAsync(ct);

        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
    }
}
