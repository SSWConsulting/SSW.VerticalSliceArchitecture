namespace SSW.VerticalSliceArchitecture.Features.Heroes.GetAllHeroes;

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
