namespace SSW.VerticalSliceArchitecture.Features.Teams.CompleteMission;

public class CompleteMissionInputValidator : AbstractValidator<CompleteMissionInput>
{
    public CompleteMissionInputValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();
    }
}
