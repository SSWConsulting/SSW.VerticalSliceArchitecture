using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace SSW.VerticalSliceArchitecture.Features.Teams;

public sealed class TeamsFeature : IFeature
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        // TODO: Add feature-specific services here
    }
}

public class TeamsGroup : Group
{
    public TeamsGroup()
    {
        // NOTE: The prefix is used as the tag and group name
        base.Configure("teams", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}