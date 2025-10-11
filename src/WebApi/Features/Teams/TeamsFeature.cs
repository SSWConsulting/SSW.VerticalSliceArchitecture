using FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Features.Teams;

public sealed class TeamsFeature : IFeature
{
    public static string FeatureName => "Teams";

    public static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {

    }
}

public class TeamsGroup : Group
{
    public TeamsGroup()
    {
        Configure("teams", ep =>
        {
            ep.Description(x => x
                .WithGroupName("Teams")
                .WithTags("Teams"));

        });
    }
}