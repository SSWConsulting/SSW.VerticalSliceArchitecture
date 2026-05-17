namespace SSW.VerticalSliceArchitecture.Features.Heroes.UpdateHero;

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
