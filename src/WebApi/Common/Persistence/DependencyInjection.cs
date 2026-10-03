using EntityFramework.Exceptions.SqlServer;
using SSW.VerticalSliceArchitecture.Common.Persistence.Interceptors;

namespace SSW.VerticalSliceArchitecture.Common.Persistence;

public static class DependencyInjection
{
    public const string ConnectionName = "AppDb";

    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddScoped<EntitySaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddSingleton(TimeProvider.System);

        var connectionString = builder.Configuration.GetConnectionString(ConnectionName)
                               ?? throw new InvalidOperationException(
                                   $"Connection string '{ConnectionName}' was not found. Aspire supplies it to the api resource.");

        // A factory, not a plain AddDbContext, because GraphQL resolves sibling fields in
        // parallel and a DbContext allows one operation at a time. HotChocolate hands each
        // resolver its own context from this factory (see RegisterDbContextFactory in GraphQlExt)
        // and disposes it when the request ends.
        //
        // Scoped, not the default singleton lifetime: the interceptors below depend on
        // ICurrentUserService, which is scoped because it reads the current HTTP request.
        services.AddDbContextFactory<ApplicationDbContext>(
            (serviceProvider, options) =>
            {
                options.UseSqlServer(connectionString);

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<EntitySaveChangesInterceptor>(),
                    serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>());

                // Return strongly typed useful exceptions
                options.UseExceptionProcessor();
            },
            ServiceLifetime.Scoped);

        // Everything outside a resolver — the interceptors, the eventual consistency middleware,
        // the seeder, the integration tests — still asks for a DbContext directly.
        services.AddScoped<ApplicationDbContext>(serviceProvider =>
            serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

        // Adds the retry policy, health check and telemetry that AddSqlServerDbContext would have
        // added. That call registers its own scoped DbContext, which is why it is not used here.
        builder.EnrichSqlServerDbContext<ApplicationDbContext>();
    }
}
