namespace SSW.VerticalSliceArchitecture.Features.Teams.AddHeroToTeam;

public class AddHeroToTeamInputValidator : AbstractValidator<AddHeroToTeamInput>
{
    public AddHeroToTeamInputValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();

        RuleFor(v => v.HeroId)
            .NotEmpty();
    }
}
