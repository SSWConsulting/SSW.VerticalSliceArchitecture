using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.Heroes.GetAllHeroes;

public class GetAllHeroesRequestValidator : PagedRequestValidator<GetAllHeroesRequest, Hero>
{
    public GetAllHeroesRequestValidator()
        : base(HeroSpec.SortColumns)
    {
    }
}
