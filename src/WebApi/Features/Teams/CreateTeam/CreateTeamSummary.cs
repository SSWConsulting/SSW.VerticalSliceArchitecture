namespace SSW.VerticalSliceArchitecture.Features.Teams.CreateTeam;

public class CreateTeamSummary : Summary<CreateTeamEndpoint>
{
    public CreateTeamSummary()
    {
        Summary = "Create a new team";
        Description = "Creates a new superhero team with the specified name. Heroes can be added to the team later.";

        ExampleRequest = new CreateTeamRequest("Avengers");

        // Also, add response examples if needed
    }
}
