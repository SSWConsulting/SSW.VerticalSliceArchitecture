namespace SSW.VerticalSliceArchitecture.Features.Heroes.CreateHero;

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
