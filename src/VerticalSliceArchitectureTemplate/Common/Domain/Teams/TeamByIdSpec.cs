namespace VerticalSliceArchitectureTemplate.Common.Domain.Teams;

// For more on the Specification Pattern see: https://www.ssw.com.au/rules/use-specification-pattern/
public sealed class TeamByIdSpec : SingleResultSpecification<Team>
{
    public TeamByIdSpec(VerticalSliceArchitectureTemplate.Common.Domain.Teams.TeamId teamId)
    {
        Query.Where(t => t.Id == teamId)
            .Include(t => t.Missions)
            .Include(t => t.Heroes);
    }
}