using SSW.VerticalSliceArchitecture.Common.GraphQL.Errors;
using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero;

[MutationType]
public static partial class CreateHeroMutation
{
    /// <summary>Creates a hero and their powers.</summary>
    /// <remarks>
    /// The mutation conventions wrap this: the parameter becomes the <c>CreateHeroInput</c> argument,
    /// the return value becomes <c>CreateHeroPayload.hero</c>, and each <c>[Error]</c> adds a member
    /// to <c>CreateHeroError</c>. Services in the signature stay out of the schema.
    /// </remarks>
    [Error<InputValidationError>]
    public static async Task<Hero> CreateHeroAsync(
        CreateHeroInput input,
        IValidator<CreateHeroInput> validator,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(input, cancellationToken);

        var hero = Hero.Create(input.Name, input.Alias);
        var powers = input.Powers.Select(p => new Power(p.Name, p.PowerLevel));
        hero.UpdatePowers(powers);

        dbContext.Heroes.Add(hero);
        await dbContext.SaveChangesAsync(cancellationToken);

        return hero;
    }
}
