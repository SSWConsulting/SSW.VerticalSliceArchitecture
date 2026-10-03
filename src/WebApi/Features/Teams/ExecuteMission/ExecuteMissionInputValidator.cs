using SSW.VerticalSliceArchitecture.Common.Validation;
using SSW.VerticalSliceArchitecture.Domain.Teams;

namespace SSW.VerticalSliceArchitecture.Features.Teams.ExecuteMission;

public class ExecuteMissionInputValidator : AbstractValidator<ExecuteMissionInput>
{
    public ExecuteMissionInputValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmptyId(id => id.Value);

        RuleFor(v => v.Description)
            .NotEmpty()
            .MaximumLength(Mission.DescriptionMaxLength);
    }
}
