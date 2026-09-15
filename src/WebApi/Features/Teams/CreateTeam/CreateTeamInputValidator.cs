using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.CreateTeam;

public class CreateTeamInputValidator : AbstractValidator<CreateTeamInput>
{
    public CreateTeamInputValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(Team.NameMaxLength);
    }
}
