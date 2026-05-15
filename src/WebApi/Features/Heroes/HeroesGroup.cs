namespace SSW.VerticalSliceArchitecture.Features.Heroes;

public class HeroesGroup : Group
{
    public HeroesGroup()
    {
        // NOTE: The prefix is used as the tag and group name
        base.Configure("heroes", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}
