using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VerticalSliceArchitectureTemplate.Common.HealthChecks.EntityFrameworkDbContextHealthCheck;

public static class EntityFrameworkDbContextHealthCheckExtensions
{
    public static IHealthChecksBuilder AddEntityFrameworkDbContextCheck<TContext>(
        this IHealthChecksBuilder builder,
        string? name,
        Func<TContext, CancellationToken, Task<DbHealthCheckResult>> testQuery,
        IEnumerable<string>? tags = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(testQuery, nameof(testQuery));

        name ??= typeof(TContext).Name;

        builder.Services.Configure<EntityFrameworkDbContextHealthCheckOptions<TContext>>(name, options => options.TestQuery = testQuery);

        return builder.AddCheck<EntityFrameworkDbContextHealthCheck<TContext>>(name, HealthStatus.Unhealthy, tags ?? []);
    }
}