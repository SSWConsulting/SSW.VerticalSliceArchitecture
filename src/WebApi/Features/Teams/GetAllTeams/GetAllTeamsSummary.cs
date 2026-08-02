using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.Teams.GetAllTeams;

public class GetAllTeamsSummary : Summary<GetAllTeamsEndpoint>
{
    public GetAllTeamsSummary()
    {
        Summary = "Get a page of teams";
        Description = "Retrieves a page of superhero teams, wrapped in the standard paged envelope " +
                      "(items plus page, pageSize, totalCount, totalPages).";

        Params[nameof(GetAllTeamsRequest.Page)] =
            $"1-based page number. Defaults to {PagingParams.FirstPage}; anything lower is treated as the first page.";
        Params[nameof(GetAllTeamsRequest.PageSize)] =
            $"Items per page. Defaults to {PagingParams.DefaultPageSize} and is clamped to at most {PagingParams.MaxPageSize}.";
        Params[nameof(GetAllTeamsRequest.SortBy)] =
            $"Column to sort by — one of: {string.Join(", ", TeamSpec.SortColumns.AllowedColumns)}. Anything else returns 400.";
        Params[nameof(GetAllTeamsRequest.SortDirection)] =
            $"Sort direction — one of: {string.Join(", ", SortDirections.Allowed)}. Defaults to ascending.";

        Response<PagedList<GetAllTeamsResponse>>(200, "Teams retrieved successfully",
            example: new PagedList<GetAllTeamsResponse>(
                Items:
                [
                    new GetAllTeamsResponse(
                        Id: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                        Name: "Avengers",
                        TotalPowerLevel: 33),
                    new GetAllTeamsResponse(
                        Id: Guid.Parse("5fb85f64-5717-4562-b3fc-2c963f66afa7"),
                        Name: "X-Men",
                        TotalPowerLevel: 27)
                ],
                Page: 1,
                PageSize: 10,
                TotalCount: 2));

        Response(400, "Unknown sort column or sort direction");
        Response(500, "Internal server error");
    }
}
