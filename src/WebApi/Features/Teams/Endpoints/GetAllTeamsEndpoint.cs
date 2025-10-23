namespace SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;

public record GetAllTeamsResponse(List<GetAllTeamsResponse.TeamDto> Teams)
{
    public record TeamDto(Guid Id, string Name);
}

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

public class GetAllTeamsSummary : Summary<GetAllTeamsEndpoint>
{
    public GetAllTeamsSummary()
    {
        Summary = "Get all teams";
        Description = "Retrieves a list of all superhero teams.";
        
        Response<GetAllTeamsResponse>(200, "Teams retrieved successfully",
            example: new GetAllTeamsResponse(
            [
                new GetAllTeamsResponse.TeamDto(
                    Id: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    Name: "Avengers"),
                new GetAllTeamsResponse.TeamDto(
                    Id: Guid.Parse("5fb85f64-5717-4562-b3fc-2c963f66afa7"),
                    Name: "X-Men")
            ]));
        
        Response(500, "Internal server error");
    }
}