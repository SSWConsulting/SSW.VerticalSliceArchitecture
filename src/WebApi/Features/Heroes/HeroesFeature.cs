using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace SSW.VerticalSliceArchitecture.Features.Heroes;

public sealed class HeroesFeature : IFeature
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        // TODO: Add feature-specific services here
    }
}

public class HeroesGroup : Group
{
    public HeroesGroup()
    {
        // NOTE: The prefix is used as the tag and group name
        base.Configure("heroes", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}