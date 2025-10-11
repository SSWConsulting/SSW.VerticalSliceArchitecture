using FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Heroes;

public sealed class HeroesFeature : IFeature
{
    public static string FeatureName => "Heroes";

    public static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {

    }
}

public class HeroesGroup : Group
{
    public HeroesGroup()
    {
        base.Configure("heroes", ep =>
        {
            ep.Description(x => x
                // .WithGroupName("Heroes")
                // .WithTags("Heroes")
                .ProducesProblemDetails(500)
            );
            ep.AllowAnonymous();
        });
    }
}