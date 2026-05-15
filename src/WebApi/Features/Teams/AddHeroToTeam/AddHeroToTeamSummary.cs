namespace SSW.VerticalSliceArchitecture.Features.Teams.AddHeroToTeam;

public class AddHeroToTeamSummary : Summary<AddHeroToTeamEndpoint>
{
    public AddHeroToTeamSummary()
    {
        Summary = "Add a hero to a team";
        Description = "Adds an existing hero to an existing team. Both the team and hero must exist.";
    }
}
