using SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;
using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

[MutationType]
public static partial class UpdateHeroMutation
{
    /// <summary>Replaces a hero's name, alias and powers.</summary>
    [Error<InputValidationError>]
    [Error<NotFoundError>]
    public static async Task<Hero> UpdateHeroAsync(
        UpdateHeroInput input,
        IValidator<UpdateHeroInput> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);

        var hero = await dbContext.Heroes
                       .FirstOrDefaultAsync(h => h.Id == input.HeroId, cancellationToken)
                   ?? throw new NotFoundException(HeroErrors.NotFound);

        hero.Name = input.Name;
        hero.Alias = input.Alias;
        var powers = input.Powers.Select(p => new Power(p.Name, p.PowerLevel));
        hero.UpdatePowers(powers);

        await dbContext.SaveChangesAsync(cancellationToken);

        return hero;
    }
}
