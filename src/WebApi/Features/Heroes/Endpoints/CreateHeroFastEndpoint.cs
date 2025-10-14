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

public class CreateHeroFastEndpoint(ApplicationDbContext dbContext, IFastEndpointEventPublisher eventPublisher) 
    : Endpoint<CreateHeroRequest, CreateHeroResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<HeroesGroup>();
        Description(x => x
                .WithName("CreateHeroFast")
                .WithDescription("Creates a new hero"));
    }

    public override async Task HandleAsync(CreateHeroRequest req, CancellationToken ct)
    {
        var hero = Hero.Create(req.Name, req.Alias);
        var powers = req.Powers.Select(p => new Power(p.Name, p.PowerLevel));
        hero.UpdatePowers(powers);

        await dbContext.Heroes.AddAsync(hero, ct);
        await dbContext.SaveChangesAsync(ct);

        // DM: Get events publishing via EF Interceptor
        
        // Queue domain events for eventual consistency processing
        // These will be processed by EventualConsistencyMiddleware after response is sent
        var domainEvents = hero.PopDomainEvents();
        foreach (var domainEvent in domainEvents)
        {
            eventPublisher.QueueDomainEvent(domainEvent);
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

public class CreateHeroSummary : Summary<CreateHeroFastEndpoint>
{
    public CreateHeroSummary()
    {
        Summary = "Create a new hero";
        Description = "Creates a new hero with the specified name, alias, and powers. Returns the ID of the created hero.";
        
        // Request example
        ExampleRequest = new CreateHeroRequest(
            Name: "Peter Parker",
            Alias: "Spider-Man",
            Powers:
            [
                new CreateHeroRequest.HeroPowerDto("Web Slinging", 85),
                new CreateHeroRequest.HeroPowerDto("Spider Sense", 90),
                new CreateHeroRequest.HeroPowerDto("Wall Crawling", 95)
            ]);
        
        // Response examples
        Response<CreateHeroResponse>(201, "Hero created successfully", 
            example: new CreateHeroResponse(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6")));
        
        Response(400, "Invalid request - validation failed");
        Response(500, "Internal server error");
    }
}