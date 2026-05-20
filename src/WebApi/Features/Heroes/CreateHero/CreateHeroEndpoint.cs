using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero;

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

        dbContext.Heroes.Add(hero);
        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new CreateHeroResponse(hero.Id.Value), ct);
    }
}
