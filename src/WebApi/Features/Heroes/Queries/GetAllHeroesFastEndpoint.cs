using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.Queries;

public record GetAllHeroesHeroDto(Guid Id, string Name, string Alias, int PowerLevel, IReadOnlyList<GetAllHeroesHeroPowerDto> Powers);

public record GetAllHeroesHeroPowerDto(string Name, int PowerLevel);

public class GetAllHeroesFastEndpoint : EndpointBase<IReadOnlyList<GetAllHeroesHeroDto>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetAllHeroesFastEndpoint(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/heroes");
        Group<HeroesGroup>();
        AllowAnonymous();
        Description(x => x
            .WithName("GetAllHeroesFast")
            .WithTags("Heroes")
            .Produces<IReadOnlyList<GetAllHeroesHeroDto>>(200)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var heroes = await _dbContext.Heroes
            .Select(h => new GetAllHeroesHeroDto(
                h.Id.Value,
                h.Name,
                h.Alias,
                h.PowerLevel,
                h.Powers.Select(p => new GetAllHeroesHeroPowerDto(p.Name, p.PowerLevel)).ToList()))
            .ToListAsync(ct);

        await HttpContext.Response.WriteAsJsonAsync(heroes, ct);
    }
}
