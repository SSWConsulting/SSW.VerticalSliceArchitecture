using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VerticalSliceArchitectureTemplate.Common.Interceptors;

namespace VerticalSliceArchitectureTemplate.Common.Persistence;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddSqlServerDbContext<AppDbContext>("app-db",
            null,
            options =>
            {
                var serviceProvider = builder.Services.BuildServiceProvider();
                options.AddInterceptors(
                    serviceProvider.GetRequiredService<EntitySaveChangesInterceptor>(),
                    serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>());

                // Return strongly typed useful exceptions
                options.UseExceptionProcessor();
            });

        var services = builder.Services;

        services.AddScoped<AppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        
        services.AddScoped<EntitySaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddSingleton(TimeProvider.System);
    }
}