namespace SSW.VerticalSliceArchitecture.Features.Teams;

public class TeamsGroup : Group
{
    public TeamsGroup()
    {
        // NOTE: The prefix is used as the tag and group name
        base.Configure("teams", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}
