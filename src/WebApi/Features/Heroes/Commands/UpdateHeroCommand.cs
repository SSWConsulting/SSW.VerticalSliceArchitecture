using MediatR;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Host.Extensions;
using System.Text.Json.Serialization;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.Commands;

public static class UpdateHeroCommand
{
    public record UpdateHeroPowerDto(string Name, int PowerLevel);
    
    public record Request(
        string Name,
        string Alias,
        IEnumerable<UpdateHeroPowerDto> Powers) : IRequest<ErrorOr<Guid>>
    {
        [JsonIgnore]
        public Guid HeroId { get; set; }
    }
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(HeroesFeature.FeatureName)
                .MapPut("/{heroId:guid}",
                    async (ISender sender, Guid heroId, Request request, CancellationToken cancellationToken) =>
                    {
                        request.HeroId = heroId;
                        var result = await sender.Send(request, cancellationToken);
                        return result.Match(_ => TypedResults.NoContent(), CustomResult.Problem);
                    })
                .WithName("UpdateHero")
                .ProducesPut();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(v => v.HeroId)
                .NotEmpty();

            RuleFor(v => v.Name)
                .NotEmpty();

            RuleFor(v => v.Alias)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, ErrorOr<Guid>>
    {
        public async Task<ErrorOr<Guid>> Handle(Request request, CancellationToken cancellationToken)
        {
            var heroId = HeroId.From(request.HeroId);
            var hero = await dbContext.Heroes
                .Include(h => h.Powers)
                .FirstOrDefaultAsync(h => h.Id == heroId, cancellationToken);

            if (hero is null)
                return HeroErrors.NotFound;

            hero.Name = request.Name;
            hero.Alias = request.Alias;
            var powers = request.Powers.Select(p => new Power(p.Name, p.PowerLevel));
            hero.UpdatePowers(powers);

            await dbContext.SaveChangesAsync(cancellationToken);

            return hero.Id.Value;
        }
    }
}