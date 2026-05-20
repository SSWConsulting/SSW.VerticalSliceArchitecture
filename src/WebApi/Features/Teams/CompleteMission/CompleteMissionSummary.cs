namespace SSW.VerticalSliceArchitecture.Features.Teams.CompleteMission;

public class CompleteMissionSummary : Summary<CompleteMissionEndpoint>
{
    public CompleteMissionSummary()
    {
        Summary = "Complete the current mission";
        Description = "Marks the team's current mission as completed. The team must have an active mission.";
    }
}
