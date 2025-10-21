namespace SSW.VerticalSliceArchitecture.Features.Entities;

public sealed class EntitiesFeature : IFeature
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        // TODO: Add feature-specific services here
    }
}

public class EntitiesGroup : Group
{
    public EntitiesGroup()
    {
        // NOTE: The prefix is used as the tag and group name
        base.Configure("entities", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}