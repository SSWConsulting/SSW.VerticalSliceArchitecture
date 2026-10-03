using SSW.VerticalSliceArchitecture.Common.Validation;

namespace SSW.VerticalSliceArchitecture.Features.Teams.CompleteMission;

public class CompleteMissionInputValidator : AbstractValidator<CompleteMissionInput>
{
    public CompleteMissionInputValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmptyId(id => id.Value);
    }
}
