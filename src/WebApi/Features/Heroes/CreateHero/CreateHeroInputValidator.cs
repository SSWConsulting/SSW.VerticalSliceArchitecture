using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero;

public class CreateHeroInputValidator : AbstractValidator<CreateHeroInput>
{
    public CreateHeroInputValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(Hero.NameMaxLength);

        RuleFor(v => v.Alias)
            .NotEmpty()
            .MaximumLength(Hero.AliasMaxLength);

        // RuleForEach silently passes over a null collection, and the resolver enumerates
        // Powers unconditionally, so without this a request omitting "powers" faults the resolver.
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
