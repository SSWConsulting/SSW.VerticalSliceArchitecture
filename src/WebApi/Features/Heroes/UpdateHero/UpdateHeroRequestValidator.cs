using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

public class UpdateHeroRequestValidator : Validator<UpdateHeroRequest>
{
    public UpdateHeroRequestValidator()
    {
        RuleFor(v => v.HeroId)
            .NotEmpty();

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(Hero.NameMaxLength);

        RuleFor(v => v.Alias)
            .NotEmpty()
            .MaximumLength(Hero.AliasMaxLength);

        // RuleForEach silently passes over a null collection, and the endpoint enumerates
        // Powers unconditionally, so without this a body omitting "powers" is a 500 not a 400.
        RuleFor(v => v.Powers)
            .NotNull();

        RuleForEach(v => v.Powers)
            .ChildRules(power =>
            {
                power.RuleFor(p => p.Name)
                    .NotEmpty()
                    .MaximumLength(Power.NameMaxLength);

                power.RuleFor(p => p.PowerLevel)
                    .InclusiveBetween(1, 10);
            });
    }
}
