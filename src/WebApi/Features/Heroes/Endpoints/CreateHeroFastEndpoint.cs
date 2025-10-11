using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.Endpoints;

public record CreateHeroRequest(
    string Name,
    string Alias,
    IEnumerable<CreateHeroRequest.HeroPowerDto> Powers)
{
    public record HeroPowerDto(string Name, int PowerLevel);
}

public record CreateHeroResponse(Guid Id);

public class CreateHeroFastEndpoint : EndpointBase<CreateHeroRequest, CreateHeroResponse>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFastEndpointEventPublisher _eventPublisher;

    public CreateHeroFastEndpoint(ApplicationDbContext dbContext, IFastEndpointEventPublisher eventPublisher)
    {
        _dbContext = dbContext;
        _eventPublisher = eventPublisher;
    }

    public override void Configure()
    {
        // DM: Check the group stuff works
        Post("/heroes");
        Group<HeroesGroup>();
        Description(x => x
            .WithName("CreateHeroFast")
            .WithTags("Heroes")
            .Produces<CreateHeroResponse>(201)
            .ProducesProblemDetails(400)
            .ProducesProblemDetails(500));
    }

    public override async Task HandleAsync(CreateHeroRequest req, CancellationToken ct)
    {
        var hero = Hero.Create(req.Name, req.Alias);
        var powers = req.Powers.Select(p => new Power(p.Name, p.PowerLevel));
        hero.UpdatePowers(powers);

        await _dbContext.Heroes.AddAsync(hero, ct);
        await _dbContext.SaveChangesAsync(ct);

        // DM: Get events publishing via EF Interceptor
        
        // Queue domain events for eventual consistency processing
        // These will be processed by EventualConsistencyMiddleware after response is sent
        var domainEvents = hero.PopDomainEvents();
        foreach (var domainEvent in domainEvents)
        {
            _eventPublisher.QueueDomainEvent(domainEvent);
        }

        // DM: Look at sending a CreatedAt response
        await Send.OkAsync(new CreateHeroResponse(hero.Id.Value), ct);
    }
}

public class CreateHeroRequestValidator : Validator<CreateHeroRequest>
{
    public CreateHeroRequestValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty();

        RuleFor(v => v.Alias)
            .NotEmpty();
    }
}