namespace VerticalSliceArchitectureTemplate;

// SM: Correct location for this class?
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(applicationAssembly, includeInternalTypes: true);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(applicationAssembly);
            // SM: Add Behaviours?
            // config.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));

            // NOTE: Switch to ValidationExceptionBehavior if you want to use exceptions over the result pattern for flow control
            //config.AddOpenBehavior(typeof(ValidationExceptionBehaviour<,>));
            // config.AddOpenBehavior(typeof(ValidationErrorOrResultBehavior<,>));

            // config.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });
        
        return services;
    }
}