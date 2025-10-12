using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record GetTeamResponse(Guid Id, string Name, IEnumerable<GetTeamResponse.GetTeamHeroDto> Heroes)
{
    public record GetTeamHeroDto(Guid Id, string Name, string Alias, int PowerLevel, IEnumerable<GetTeamHeroPowerDto> Powers);

    public record GetTeamHeroPowerDto(string Name, int PowerLevel);
}

public record GetTeamRequest(Guid TeamId);

public class GetTeamFastEndpoint(ApplicationDbContext dbContext) 
    : Endpoint<GetTeamRequest, GetTeamResponse>
{
    public override void Configure()
    {
        Get("/{teamId}");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("GetTeamFast")
            .Produces(404));
    }

    public override async Task HandleAsync(GetTeamRequest req, CancellationToken ct)
    {
        var teamId = TeamId.From(req.TeamId);

        var team = await dbContext.Teams
            .Where(t => t.Id == teamId)
            .Select(t => new GetTeamResponse(
                t.Id.Value,
                t.Name,
                t.Heroes.Select(
                    h => new GetTeamResponse.GetTeamHeroDto(
                        h.Id.Value, 
                        h.Name, 
                        h.Alias, 
                        h.PowerLevel, 
                        h.Powers.Select(p => new GetTeamResponse.GetTeamHeroPowerDto(p.Name, p.PowerLevel))
                    )
                )))
            .FirstOrDefaultAsync(ct);

        if (team is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(team, ct);
    }
}

public class GetTeamRequestValidator : Validator<GetTeamRequest>
{
    public GetTeamRequestValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();
    }
}