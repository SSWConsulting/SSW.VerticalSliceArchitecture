using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.Endpoints;

public record CreateHeroRequest(
    string Name,
    string Alias,
    IEnumerable<CreateHeroRequest.HeroPowerDto> Powers)
{
    public record HeroPowerDto(string Name, int PowerLevel);
}

public record CreateHeroResponse(Guid Id);

public class CreateHeroEndpoint(ApplicationDbContext dbContext)
    : Endpoint<CreateHeroRequest, CreateHeroResponse>
{
    public override void Configure()
    {
        Post("/");
        Group<HeroesGroup>();
        Description(x => x.WithName("CreateHero"));
    }

    public override async Task HandleAsync(CreateHeroRequest req, CancellationToken ct)
    {
        var hero = Hero.Create(req.Name, req.Alias);
        var powers = req.Powers.Select(p => new Power(p.Name, p.PowerLevel));
        hero.UpdatePowers(powers);

        await dbContext.Heroes.AddAsync(hero, ct);
        await dbContext.SaveChangesAsync(ct);

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

        RuleForEach(v => v.Powers)
            .ChildRules(power =>
            {
                power.RuleFor(p => p.PowerLevel)
                    .InclusiveBetween(1, 10);
            });
    }
}

public class CreateHeroSummary : Summary<CreateHeroEndpoint>
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
                new CreateHeroRequest.HeroPowerDto("Web Slinging", 1),
                new CreateHeroRequest.HeroPowerDto("Spider Sense", 7),
                new CreateHeroRequest.HeroPowerDto("Wall Crawling", 10)
            ]);

        // Also, add response examples if needed
    }
}