using System.Reflection;

namespace SSW.VerticalSliceArchitecture.Common.Events;

public static class DomainEventHandlerRegistration
{
    /// <summary>
    /// Registers the dispatcher and every <see cref="IDomainEventHandler{TEvent}"/> in the assembly.
    /// </summary>
    /// <remarks>
    /// Handlers are discovered rather than listed so that adding a slice with a handler needs no edit
    /// to a central file — the same reason feature discovery exists.
    /// </remarks>
    public static IServiceCollection AddDomainEvents(this IServiceCollection services, Assembly assembly)
    {
        ThrowIfNull(assembly);

        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();

        var handlers = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .SelectMany(
                t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                    .Select(i => (Service: i, Implementation: t)));

        foreach (var (service, implementation) in handlers)
            services.AddScoped(service, implementation);

        return services;
    }
}
