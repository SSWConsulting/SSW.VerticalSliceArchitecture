using System.Reflection;

namespace VerticalSliceArchitecture;

public static class EndpointDiscovery
{
    public static void RegisterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => typeof(IEndpoint).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

        foreach (var type in types)
        {
            var endpoint = Activator.CreateInstance(type) as IEndpoint;
            endpoint?.MapEndpoint(endpoints);
        }
    }
}