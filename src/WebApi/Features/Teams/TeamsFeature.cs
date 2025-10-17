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
        base.Configure("teams", ep =>
        {
            ep.Description(x => x
                // .WithGroupName("teams")
                // .WithTags("teams")
                .ProducesProblemDetails(500)
            );
            ep.AllowAnonymous();
        });
    }

    protected override void Configure(string routePrefix, Action<EndpointDefinition> ep)
    {
        base.Configure(routePrefix, ep);
    }
}