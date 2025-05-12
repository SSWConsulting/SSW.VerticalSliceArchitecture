﻿using Microsoft.EntityFrameworkCore.Diagnostics;

namespace VerticalSliceArchitectureTemplate.Common.Persistence;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, EventPublisher>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseInMemoryDatabase("InMemoryDbForTesting");

            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });
    }
}