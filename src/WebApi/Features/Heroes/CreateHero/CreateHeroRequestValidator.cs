using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero;

public class CreateHeroRequestValidator : Validator<CreateHeroRequest>
{
    public CreateHeroRequestValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(Hero.NameMaxLength);

        RuleFor(v => v.Alias)
            .NotEmpty()
            .MaximumLength(Hero.AliasMaxLength);

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
