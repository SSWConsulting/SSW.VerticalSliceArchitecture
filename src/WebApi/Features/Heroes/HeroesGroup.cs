using FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Heroes;

public class HeroesGroup : Group
{
    public HeroesGroup()
    {
        Configure("heroes", ep =>
        {
            ep.Description(x => x
                .WithGroupName("Heroes")
                .WithTags("Heroes"));
        });
    }
}
