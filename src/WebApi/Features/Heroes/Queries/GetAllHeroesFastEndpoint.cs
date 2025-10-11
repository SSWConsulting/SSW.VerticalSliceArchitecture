using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.Queries;

public static class GetAllHeroesFastEndpoint
{
    public record HeroDto(Guid Id, string Name, string Alias, int PowerLevel, IReadOnlyList<HeroPowerDto> Powers);
    public record HeroPowerDto(string Name, int PowerLevel);
    
    public class Endpoint : EndpointBase<IReadOnlyList<HeroDto>>
    {
        private readonly ApplicationDbContext _dbContext;

        public Endpoint(ApplicationDbContext dbContext)
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
                .Produces<IReadOnlyList<HeroDto>>(200)
                .ProducesProblemDetails(500));
        }

        public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
        {
            var heroes = await _dbContext.Heroes
                .Select(h => new HeroDto(
                    h.Id.Value,
                    h.Name,
                    h.Alias,
                    h.PowerLevel,
                    h.Powers.Select(p => new HeroPowerDto(p.Name, p.PowerLevel)).ToList()))
                .ToListAsync(ct);

            await HttpContext.Response.WriteAsJsonAsync(heroes, ct);
        }
    }
}
