namespace SSW.VerticalSliceArchitecture.Features.Heroes.Endpoints;

public record GetAllHeroesResponse(List<GetAllHeroesResponse.HeroDto> Heroes)
{
    public record HeroDto(
        Guid Id,
        string Name,
        string Alias,
        int PowerLevel,
        IReadOnlyList<HeroPowerDto> Powers);

    public record HeroPowerDto(string Name, int PowerLevel);
}

public class GetAllHeroesEndpoint(ApplicationDbContext dbContext) 
    : EndpointWithoutRequest<GetAllHeroesResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<HeroesGroup>();
        Description(x => x.WithName("GetAllHeroes"));
    }

    public async override Task HandleAsync(CancellationToken ct)
    {
        var heroes = await dbContext.Heroes
            .Select(h => new GetAllHeroesResponse.HeroDto(
                h.Id.Value,
                h.Name,
                h.Alias,
                h.PowerLevel,
                h.Powers.Select(p => new GetAllHeroesResponse.HeroPowerDto(p.Name, p.PowerLevel)).ToList()))
            .ToListAsync(ct);

        await Send.OkAsync(new GetAllHeroesResponse(heroes), ct);
    }
}

public class GetAllHeroesSummary : Summary<GetAllHeroesEndpoint>
{
    public GetAllHeroesSummary()
    {
        Summary = "Get all heroes";
        Description = "Retrieves a list of all heroes with their powers and power levels.";
        
        // Response example
        Response(200, "Heroes retrieved successfully",
            example: new GetAllHeroesResponse(
            [
                new GetAllHeroesResponse.HeroDto(
                    Id: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    Name: "Peter Parker",
                    Alias: "Spider-Man",
                    PowerLevel: 15,
                    Powers:
                    [
                        new GetAllHeroesResponse.HeroPowerDto("Web Slinging", 3),
                        new GetAllHeroesResponse.HeroPowerDto("Spider Sense", 5),
                        new GetAllHeroesResponse.HeroPowerDto("Wall Crawling", 7)
                    ]),
                new GetAllHeroesResponse.HeroDto(
                    Id: Guid.Parse("5fb85f64-5717-4562-b3fc-2c963f66afa7"),
                    Name: "Tony Stark",
                    Alias: "Iron Man",
                    PowerLevel: 18,
                    Powers:
                    [
                        new GetAllHeroesResponse.HeroPowerDto("Flight", 4),
                        new GetAllHeroesResponse.HeroPowerDto("Repulsor Rays", 6),
                        new GetAllHeroesResponse.HeroPowerDto("Super Strength", 8)
                    ])
            ]));

        // Also, add response examples if needed
    }
}