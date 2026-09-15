using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.ExecuteMission;

public class ExecuteMissionInputValidator : AbstractValidator<ExecuteMissionInput>
{
    public ExecuteMissionInputValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty();

        RuleFor(v => v.Description)
            .NotEmpty()
            .MaximumLength(Mission.DescriptionMaxLength);
    }
}
