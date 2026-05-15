namespace SSW.VerticalSliceArchitecture.Features.Teams.GetTeam;

public class GetTeamRequestValidator : Validator<GetTeamRequest>
{
    public GetTeamRequestValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();
    }
}
