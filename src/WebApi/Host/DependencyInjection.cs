using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Behaviours;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Interfaces;
using SSW.VerticalSliceArchitecture.Common.Services;

namespace SSW.VerticalSliceArchitecture.Host;

public static class DependencyInjection
{
    public static void AddWebApi(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        
        services.AddHttpContextAccessor();
        
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        services.AddOpenApi();
        
        // Add FastEndpoints
        services.AddFastEndpoints(options =>
        {
            options.Assemblies = [typeof(DependencyInjection).Assembly];
        });
        
        // Register FastEndpoints event publisher for queuing domain events
        services.AddScoped<IFastEndpointEventPublisher, FastEndpointEventPublisher>();
    }
    
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;
        var services = builder.Services;
        
        services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(applicationAssembly);
            
            config.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));

            // NOTE: Switch to ValidationExceptionBehavior if you want to use exceptions over the result pattern for flow control
            // config.AddOpenBehavior(typeof(ValidationExceptionBehaviour<,>));
            config.AddOpenBehavior(typeof(ValidationErrorOrResultBehavior<,>));

            config.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
    }
}