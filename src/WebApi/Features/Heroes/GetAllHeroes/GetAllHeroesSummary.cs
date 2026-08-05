using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.GetAllHeroes;

public class GetAllHeroesSummary : Summary<GetAllHeroesEndpoint>
{
    public GetAllHeroesSummary()
    {
        Summary = "Get a page of heroes";
        Description = "Retrieves a page of heroes with their powers and power levels, wrapped in the " +
                      "standard paged envelope (items plus page, pageSize, totalCount, totalPages).";

        Params[nameof(GetAllHeroesRequest.Page)] =
            $"1-based page number. Defaults to {PagingParams.FirstPage}; anything lower is treated as the first page.";
        Params[nameof(GetAllHeroesRequest.PageSize)] =
            $"Items per page. Defaults to {PagingParams.DefaultPageSize} and is clamped to at most {PagingParams.MaxPageSize}.";
        Params[nameof(GetAllHeroesRequest.SortBy)] =
            $"Column to sort by — one of: {string.Join(", ", HeroSpec.SortColumns.AllowedColumns)}. Anything else returns 400.";
        Params[nameof(GetAllHeroesRequest.SortDirection)] =
            $"Sort direction — one of: {string.Join(", ", SortDirections.Allowed)}. Defaults to ascending.";

        // Response example
        Response(200, "Heroes retrieved successfully",
            example: new PagedList<GetAllHeroesResponse>(
                Items:
                [
                    new GetAllHeroesResponse(
                        Id: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                        Name: "Peter Parker",
                        Alias: "Spider-Man",
                        PowerLevel: 15,
                        Powers:
                        [
                            new GetAllHeroesResponse.HeroPowerDto("Web Slinging", 3),
                            new GetAllHeroesResponse.HeroPowerDto("Spider Sense", 5),
                            new GetAllHeroesResponse.HeroPowerDto("Wall Crawling", 7)
                        ]),
                    new GetAllHeroesResponse(
                        Id: Guid.Parse("5fb85f64-5717-4562-b3fc-2c963f66afa7"),
                        Name: "Tony Stark",
                        Alias: "Iron Man",
                        PowerLevel: 18,
                        Powers:
                        [
                            new GetAllHeroesResponse.HeroPowerDto("Flight", 4),
                            new GetAllHeroesResponse.HeroPowerDto("Repulsor Rays", 6),
                            new GetAllHeroesResponse.HeroPowerDto("Super Strength", 8)
                        ])
                ],
                Page: 1,
                PageSize: 10,
                TotalCount: 2));

        Response(400, "Unknown sort column or sort direction");
    }
}
