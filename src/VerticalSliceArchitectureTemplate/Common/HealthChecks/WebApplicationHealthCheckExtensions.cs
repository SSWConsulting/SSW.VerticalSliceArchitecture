using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VerticalSliceArchitectureTemplate.Common.HealthChecks.EntityFrameworkDbContextHealthCheck;

namespace VerticalSliceArchitectureTemplate.Common.HealthChecks;

public static class WebApplicationHealthCheckExtensions
{
    public static WebApplication UseHealthChecks(this WebApplication app)
    {
        // Basic Healthy/Degraded/Unhealthy result
        app.UseHealthChecks("/health");

        // Detailed Report about each check
        // TODO: Because of the detailed information, this endpoint should be secured behind
        // an Authorization Policy (.RequireAuthorization()), or a specific secured port through firewall rules
        app.UseHealthChecks("/health-report",
            new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

        return app;
    }

    public static void AddHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks()
            // Check 1: Check the SQL Server Connectivity (no EF, no DBContext, hardly anything to go wrong)
            .AddSqlServer(
                name: "SQL Server",
                connectionString: config["ConnectionStrings:DefaultConnection"]!,
                healthQuery: $"-- SqlServerHealthCheck{Environment.NewLine}SELECT 123;",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sql", "sqlserver"])

            // Check 2: Check the Entity Framework DbContext (requires the DbContext Options, DI, Interceptors, Configurations, etc. to all be correct), and
            // then run a sample query to test important data
            // Note: Add TagWith("HealthCheck") to show up in SQL Profiling tools (usually as the opening comment) so that you know that the constant DB Queries are
            // for the health check of the current application and not something strange/unidentified.
            .AddEntityFrameworkDbContextCheck<AppDbContext>(
                name: "Entity Framework DbContext",
                tags: ["db", "dbContext", "sql"],
                testQuery: async (ctx, ct) =>
                {
                    // TODO: Replace the custom test query below with something appropriate for your project that is always expected to be valid
                    _ = await ctx
                        .Heroes
                        // allows you to understand why you might see constant db queries in sql profile
                        .TagWith("HealthCheck")
                        .FirstOrDefaultAsync(ct);

                    return new DbHealthCheckResult("Database Context is healthy");
                });
    }
}