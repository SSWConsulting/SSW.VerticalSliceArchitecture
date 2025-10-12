using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;


namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record AddHeroToTeamRequest(Guid TeamId, Guid HeroId);

public class AddHeroToTeamFastEndpoint(ApplicationDbContext dbContext) 
    : Endpoint<AddHeroToTeamRequest>
{
    public override void Configure()
    {
        Post("/{teamId}/heroes/{heroId}");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("AddHeroToTeamFast")
            .Produces(404));
    }

    public override async Task HandleAsync(AddHeroToTeamRequest req, CancellationToken ct)
    {
        var teamId = TeamId.From(req.TeamId);
        var heroId = HeroId.From(req.HeroId);

        var team = dbContext.Teams
            .WithSpecification(new TeamByIdSpec(teamId))
            .FirstOrDefault();

        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var hero = dbContext.Heroes
            .WithSpecification(new HeroByIdSpec(heroId))
            .FirstOrDefault();

        if (hero is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        team.AddHero(hero);
        await dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
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