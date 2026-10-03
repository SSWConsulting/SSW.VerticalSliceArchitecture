using SSW.VerticalSliceArchitecture.Common.Validation;
using SSW.VerticalSliceArchitecture.Domain.Heroes;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

public class UpdateHeroInputValidator : AbstractValidator<UpdateHeroInput>
{
    public UpdateHeroInputValidator()
    {
        RuleFor(v => v.HeroId)
            .NotEmptyId(id => id.Value);

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(Hero.NameMaxLength);

        RuleFor(v => v.Alias)
            .NotEmpty()
            .MaximumLength(Hero.AliasMaxLength);

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
