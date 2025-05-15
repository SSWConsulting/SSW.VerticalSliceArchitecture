using MediatR;
using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;
using VerticalSliceArchitectureTemplate.Common.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.Heroes.Commands;

public static class CreateHeroCommand
{
    public record CreateHeroPowerDto(string Name, int PowerLevel);
    
    public record Request(
        string Name,
        string Alias,
        IEnumerable<CreateHeroPowerDto> Powers) : IRequest<ErrorOr<Guid>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(HeroesFeature.FeatureName)
                .MapPost("/",
                    async (ISender sender, Request request, CancellationToken ct) =>
                    {
                        var result = await sender.Send(request, ct);
                        return result.Match(_ => TypedResults.Created(), CustomResult.Problem);
                    })
                .WithName("CreateHero")
                .ProducesPost();
        }
    }
    
    internal sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
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
            var hero = Hero.Create(request.Name, request.Alias);
            var powers = request.Powers.Select(p => new Power(p.Name, p.PowerLevel));
            hero.UpdatePowers(powers);

            await dbContext.Heroes.AddAsync(hero, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return hero.Id.Value;
        }
    }
}