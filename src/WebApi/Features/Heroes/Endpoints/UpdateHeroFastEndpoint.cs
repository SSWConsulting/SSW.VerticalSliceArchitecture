using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.Endpoints;

public record UpdateHeroRequest(
    string Name,
    string Alias,
    Guid HeroId,
    IEnumerable<UpdateHeroRequest.HeroPowerDto> Powers)
{
    public record HeroPowerDto(string Name, int PowerLevel);
}

public class UpdateHeroFastEndpoint(ApplicationDbContext dbContext)
    : Endpoint<UpdateHeroRequest>
{
    public override void Configure()
    {
        Put("/{heroId}");
        Group<HeroesGroup>();
        Description(x => x
            .WithName("UpdateHeroFast")
            .Produces(StatusCodes.Status404NotFound));
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

        await Send.NoContentAsync(ct);
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

public class UpdateHeroSummary : Summary<UpdateHeroFastEndpoint>
{
    public UpdateHeroSummary()
    {
        Summary = "Update an existing hero";
        Description = "Updates a hero's name, alias, and powers. The hero must exist.";
        
        // Request example
        ExampleRequest = new UpdateHeroRequest(
            Name: "Peter Benjamin Parker",
            Alias: "The Amazing Spider-Man",
            HeroId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Powers:
            [
                new UpdateHeroRequest.HeroPowerDto("Web Slinging", 90),
                new UpdateHeroRequest.HeroPowerDto("Spider Sense", 95),
                new UpdateHeroRequest.HeroPowerDto("Wall Crawling", 98),
                new UpdateHeroRequest.HeroPowerDto("Super Strength", 75)
            ]);
        
        // Also, add response examples if needed
    }
}