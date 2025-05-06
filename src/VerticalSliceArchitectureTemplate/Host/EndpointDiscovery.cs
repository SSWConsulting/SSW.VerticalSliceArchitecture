using System.Reflection;

namespace VerticalSliceArchitectureTemplate.Host;

// TODO: Source generate this
public static class EndpointDiscovery
{
    private static readonly Type EndpointType = typeof(IEndpoints);

    public static void RegisterEndpoints(this IEndpointRouteBuilder endpoints, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));
        }
        
        var endpointsTypes = GetEndpointTypes(assemblies);

        foreach (var type in endpointsTypes)
        {
            var method = GetMapEndpointsMethod(type);
            method?.Invoke(null, [endpoints]);
        }
    }

    private static IEnumerable<Type> GetEndpointTypes(params Assembly[] assemblies)
    {
        return assemblies.SelectMany(x => x.GetTypes())
            .Where(x => EndpointType.IsAssignableFrom(x) &&
                        x is { IsInterface: false, IsAbstract: false });
    }

    private static MethodInfo? GetMapEndpointsMethod(Type type)
    {
        return type.GetMethod(nameof(IEndpoints.MapEndpoints),
            BindingFlags.Static | BindingFlags.Public);
    }
}