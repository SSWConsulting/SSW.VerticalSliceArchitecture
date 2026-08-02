using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.GetAllHeroes;

public class GetAllHeroesEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetAllHeroesRequest, PagedList<GetAllHeroesResponse>>
{
    public override void Configure()
    {
        Get("/");
        Group<HeroesGroup>();
        Description(x => x.WithName("GetAllHeroes"));
    }

    public async override Task HandleAsync(GetAllHeroesRequest req, CancellationToken ct)
    {
        var paging = PagingParams.From(req.Page, req.PageSize);
        var spec = HeroSpec.Paged(paging, req.SortBy, SortDirections.From(req.SortDirection));

        var heroes = await dbContext.Heroes.ToPagedListAsync(
            spec,
            h => new GetAllHeroesResponse(
                h.Id.Value,
                h.Name,
                h.Alias,
                h.PowerLevel,
                h.Powers.Select(p => new GetAllHeroesResponse.HeroPowerDto(p.Name, p.PowerLevel)).ToList()),
            ct);

        await Send.OkAsync(heroes, ct);
    }
}
