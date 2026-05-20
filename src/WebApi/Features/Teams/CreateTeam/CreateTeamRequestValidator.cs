namespace SSW.VerticalSliceArchitecture.Features.Teams.CreateTeam;

public class CreateTeamRequestValidator : Validator<CreateTeamRequest>
{
    public CreateTeamRequestValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty();
    }
}
