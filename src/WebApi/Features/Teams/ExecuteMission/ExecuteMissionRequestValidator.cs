namespace SSW.VerticalSliceArchitecture.Features.Teams.ExecuteMission;

public class ExecuteMissionRequestValidator : Validator<ExecuteMissionRequest>
{
    public ExecuteMissionRequestValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();

        RuleFor(v => v.Description)
            .NotEmpty();
    }
}
