using System.Reflection;

namespace VerticalSliceArchitectureTemplate.Kernel;

public static class ModuleDiscovery
{
    private static readonly Type ModuleType = typeof(IModule);

    public static void ConfigureModules(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));
        }
        
        var moduleTypes = GetModuleTypes(assemblies);

        foreach (var type in moduleTypes)
        {
            var method = GetMapEndpointMethod(type);
            method?.Invoke(null, new object[] { services });
        }
    }
    
    private static IEnumerable<Type> GetModuleTypes(params Assembly[] assemblies)
    {
        return assemblies.SelectMany(x => x.GetTypes())
            .Where(x => ModuleType.IsAssignableFrom(x) &&
                        x is { IsInterface: false, IsAbstract: false });
    }

    private static MethodInfo? GetMapEndpointMethod(IReflect type)
    {
        return type.GetMethod(nameof(IModule.ConfigureServices),
            BindingFlags.Static | BindingFlags.Public);
    }
}