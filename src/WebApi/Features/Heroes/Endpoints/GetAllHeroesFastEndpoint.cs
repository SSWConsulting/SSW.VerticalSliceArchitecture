using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

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

public class GetAllHeroesFastEndpoint : EndpointBase<GetAllHeroesResponse>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAllHeroesFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/");
        Group<HeroesGroup>();
        AllowAnonymous();
        Description(x => x
            .WithName("GetAllHeroesFast")
            .WithDescription("Gets all heroes"));
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var heroes = await _dbContext.Heroes
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