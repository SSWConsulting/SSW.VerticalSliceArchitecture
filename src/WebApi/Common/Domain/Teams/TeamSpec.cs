namespace SSW.VerticalSliceArchitecture.Common.Domain.Teams;

// For more on the Specification Pattern see: https://www.ssw.com.au/rules/use-specification-pattern/
public sealed class TeamSpec : SingleResultSpecification<Team>
{
    public static TeamSpec ById(TeamId teamId)
    {
        var spec = new TeamSpec();
        spec.Query
            .Where(t => t.Id == teamId)
            .Include(t => t.Missions)
            .Include(t => t.Heroes);
        return spec;
    }
}
