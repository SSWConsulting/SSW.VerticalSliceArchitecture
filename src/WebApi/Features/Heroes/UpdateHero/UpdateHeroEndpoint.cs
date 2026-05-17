using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

public class UpdateHeroEndpoint(ApplicationDbContext dbContext)
    : Endpoint<UpdateHeroRequest>
{
    public override void Configure()
    {
        Put("/{heroId}");
        Group<HeroesGroup>();
        Description(x => x
            .WithName("UpdateHero")
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
