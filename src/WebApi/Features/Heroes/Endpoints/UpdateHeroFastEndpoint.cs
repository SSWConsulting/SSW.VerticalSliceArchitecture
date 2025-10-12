using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.Endpoints;


public record UpdateHeroRequest(
    string Name,
    string Alias,
    Guid HeroId,
    IEnumerable<UpdateHeroRequest.HeroPowerDto> Powers)
{
    public record HeroPowerDto(string Name, int PowerLevel);
}

public class UpdateHeroFastEndpoint(ApplicationDbContext dbContext, IFastEndpointEventPublisher eventPublisher) 
    : Endpoint<UpdateHeroRequest>
{
    public override void Configure()
    {
        Put("/{heroId}");
        Group<HeroesGroup>();
        Description(x => x
            .WithName("UpdateHeroFast")
            .WithDescription("Updates a hero"));
    }

    public override async Task HandleAsync(UpdateHeroRequest req, CancellationToken ct)
    {
        var heroId = HeroId.From(req.HeroId);
        var hero = await dbContext.Heroes
            .Include(h => h.Powers)
            .FirstOrDefaultAsync(h => h.Id == heroId, ct);

        if (hero is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        hero.Name = req.Name;
        hero.Alias = req.Alias;
        var powers = req.Powers.Select(p => new Power(p.Name, p.PowerLevel));
        hero.UpdatePowers(powers);

        await dbContext.SaveChangesAsync(ct);

        // DM: Get events publishing via EF Interceptor
        // Queue domain events for eventual consistency processing
        // These will be processed by EventualConsistencyMiddleware after response is sent
        var domainEvents = hero.PopDomainEvents();
        foreach (var domainEvent in domainEvents)
        {
            eventPublisher.QueueDomainEvent(domainEvent);
        }

        // await Send.NoContentAsync(ct);
        await Send.OkAsync(null);
    }
}

public class UpdateHeroRequestValidator : Validator<UpdateHeroRequest>
{
    public UpdateHeroRequestValidator()
    {
        RuleFor(v => v.HeroId)
            .NotEmpty();

        RuleFor(v => v.Name)
            .NotEmpty();

        RuleFor(v => v.Alias)
            .NotEmpty();
    }
}