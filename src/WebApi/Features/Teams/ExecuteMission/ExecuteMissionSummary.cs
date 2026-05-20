namespace SSW.VerticalSliceArchitecture.Features.Teams.ExecuteMission;

public class ExecuteMissionSummary : Summary<ExecuteMissionEndpoint>
{
    public ExecuteMissionSummary()
    {
        Summary = "Execute a new mission";
        Description = "Assigns a new mission to the team. The team must not have an active mission.";

        ExampleRequest = new ExecuteMissionRequest(
            TeamId: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Description: "Stop the alien invasion in New York City");
    }
}
