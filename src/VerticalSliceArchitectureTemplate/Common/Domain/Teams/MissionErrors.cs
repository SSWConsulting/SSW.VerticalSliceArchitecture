namespace VerticalSliceArchitectureTemplate.Common.Domain.Teams;

public static class MissionErrors
{
    public static readonly Error AlreadyCompleted = Error.Conflict(
        "Mission.AlreadyCompleted",
        "Mission is already completed");
}