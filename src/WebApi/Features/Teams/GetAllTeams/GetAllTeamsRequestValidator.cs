using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.Teams.GetAllTeams;

public class GetAllTeamsRequestValidator : PagedRequestValidator<GetAllTeamsRequest, Team>
{
    public GetAllTeamsRequestValidator()
        : base(TeamSpec.SortColumns)
    {
    }
}
