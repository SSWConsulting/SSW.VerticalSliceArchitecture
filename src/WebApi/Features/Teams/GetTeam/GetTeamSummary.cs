namespace SSW.VerticalSliceArchitecture.Features.Teams.GetTeam;

public class GetTeamSummary : Summary<GetTeamEndpoint>
{
    public GetTeamSummary()
    {
        Summary = "Get a specific team";
        Description = "Retrieves detailed information about a team, including all its heroes and their powers.";
    }
}
