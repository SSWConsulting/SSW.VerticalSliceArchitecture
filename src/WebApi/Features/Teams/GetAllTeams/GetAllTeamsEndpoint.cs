using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.Teams.GetAllTeams;

public class GetAllTeamsEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetAllTeamsRequest, PagedList<GetAllTeamsResponse>>
{
    public override void Configure()
    {
        Get("/");
        Group<TeamsGroup>();
        Description(x => x.WithName("GetAllTeams"));
    }

    public override async Task HandleAsync(GetAllTeamsRequest req, CancellationToken ct)
    {
        var paging = PagingParams.From(req.Page, req.PageSize);
        var spec = TeamSpec.Paged(paging, req.SortBy, SortDirections.From(req.SortDirection));

        var teams = await dbContext.Teams.ToPagedListAsync(
            spec,
            t => new GetAllTeamsResponse(t.Id.Value, t.Name, t.TotalPowerLevel),
            ct);

        await Send.OkAsync(teams, ct);
    }
}
