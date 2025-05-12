using MediatR;
using VerticalSliceArchitectureTemplate.Common.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.Heroes.Queries;

public static class GetAllHeroesQuery
{
    public record HeroDto(Guid Id, string Name, string Alias, int PowerLevel, IEnumerable<HeroPowerDto> Powers);
    public record HeroPowerDto(string Name, int PowerLevel);
    
    public record Request : IRequest<ErrorOr<IReadOnlyList<HeroDto>>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(HeroesFeature.FeatureName)
                .MapGet("/",
                    async (ISender sender, CancellationToken cancellationToken) =>
                    {
                        var request = new Request();
                        var result = await sender.Send(request, cancellationToken);
                        return TypedResults.Ok(result);
                    })
                .WithName("GetAllHeroes")
                .ProducesGet<IReadOnlyList<HeroDto>>();
        }
    }
    
    public class Validator : AbstractValidator<Request>
    {
        public Validator() {}
    }
    
    internal sealed class Handler : IRequestHandler<Request, ErrorOr<IReadOnlyList<HeroDto>>>
    {
        private readonly AppDbContext _dbContext;

        public Handler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ErrorOr<IReadOnlyList<HeroDto>>> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Heroes
                .Select(h => new HeroDto(
                    h.Id.Value,
                    h.Name,
                    h.Alias,
                    h.PowerLevel,
                    h.Powers.Select(p => new HeroPowerDto(p.Name, p.PowerLevel))))
                .ToListAsync(cancellationToken);
        }
    }
}