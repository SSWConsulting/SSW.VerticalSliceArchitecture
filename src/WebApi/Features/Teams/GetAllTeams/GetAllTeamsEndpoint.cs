namespace SSW.VerticalSliceArchitecture.Features.Teams.GetAllTeams;

public class GetAllTeamsEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<GetAllTeamsResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<TeamsGroup>();
        Description(x => x.WithName("GetAllTeams"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var teams = await dbContext.Teams
            .Select(t => new GetAllTeamsResponse.TeamDto(t.Id.Value, t.Name))
            .ToListAsync(ct);

        await Send.OkAsync(new GetAllTeamsResponse(teams), ct);
    }
}
