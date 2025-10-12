using FastEndpoints;

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

public class GetAllHeroesFastEndpoint(ApplicationDbContext dbContext) 
    : EndpointWithoutRequest<GetAllHeroesResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<HeroesGroup>();
        Description(x => x
            .WithName("GetAllHeroesFast")
            .WithDescription("Gets all heroes"));
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