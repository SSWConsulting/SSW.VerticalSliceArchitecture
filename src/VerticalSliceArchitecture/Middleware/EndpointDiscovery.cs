using System.Reflection;

namespace VerticalSliceArchitecture.Middleware;

public static class EndpointDiscovery
{
    private static readonly Type EndpointType = typeof(IEndpoint);
    public static void RegisterEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var endpointTypes = GetEndpointTypes(Assembly.GetExecutingAssembly());

        foreach (var type in endpointTypes)
        {
            var method = GetMapEndpointMethod(type);
            method?.Invoke(null, new object[] { endpoints });
        }
    }

    private static IEnumerable<Type> GetEndpointTypes(params Assembly[] assemblies) =>
       assemblies.SelectMany(x => x.GetTypes())
            .Where(x => EndpointType.IsAssignableFrom(x) &&
                        x is { IsInterface: false, IsAbstract: false });
    
    private static MethodInfo? GetMapEndpointMethod(IReflect type) =>
        type.GetMethod(nameof(IEndpoint.MapEndpoint), 
            BindingFlags.Static | BindingFlags.Public);
}