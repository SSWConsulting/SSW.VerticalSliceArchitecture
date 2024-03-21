using Microsoft.EntityFrameworkCore.Diagnostics;

namespace VerticalSliceArchitectureTemplate.Common.Persistence;

public static class DependencyInjection
{
    public static void AddEfCore(this IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, EventPublisher>();
        
        services.AddSqlServerDbContext<AppDbContext>("appdb");
    }
}