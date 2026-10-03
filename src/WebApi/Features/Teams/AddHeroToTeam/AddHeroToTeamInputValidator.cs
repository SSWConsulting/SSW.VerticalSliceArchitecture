using SSW.VerticalSliceArchitecture.Common.Validation;

namespace SSW.VerticalSliceArchitecture.Features.Teams.AddHeroToTeam;

public class AddHeroToTeamInputValidator : AbstractValidator<AddHeroToTeamInput>
{
    public AddHeroToTeamInputValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmptyId(id => id.Value);

        RuleFor(v => v.HeroId)
            .NotEmptyId(id => id.Value);
    }
}
