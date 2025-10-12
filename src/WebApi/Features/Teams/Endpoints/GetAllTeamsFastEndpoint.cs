using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record GetAllTeamsResponse(List<GetAllTeamsResponse.TeamDto> Teams)
{
    public record TeamDto(Guid Id, string Name);
}

public class GetAllTeamsFastEndpoint(ApplicationDbContext dbContext) 
    : EndpointBase<GetAllTeamsResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<TeamsGroup>();
        Description(x => x
            .WithName("GetAllTeamsFast"));
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var teams = await dbContext.Teams
            .Select(t => new GetAllTeamsResponse.TeamDto(t.Id.Value, t.Name))
            .ToListAsync(ct);

        await Send.OkAsync(new GetAllTeamsResponse(teams), ct);
    }
}