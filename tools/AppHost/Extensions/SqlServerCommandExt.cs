using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppHost.Extensions;

public static class SqlServerDatabaseCommandExt
{
    public static IResourceBuilder<SqlServerDatabaseResource> WithDropDatabaseCommand(
        this IResourceBuilder<SqlServerDatabaseResource> builder)
    {
        builder.WithCommand(
            "drop-database",
            "Drop Database",
            async context =>
            {
                var connectionString = await builder.Resource.ConnectionStringExpression.GetValueAsync(default);
                ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

                var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
                optionsBuilder.UseSqlServer(connectionString);
                var db = new DbContext(optionsBuilder.Options);
                await db.Database.EnsureDeletedAsync();

                return CommandResults.Success();
            },
            new CommandOptions {
                UpdateState = context =>
                {
                    if (context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy)
                    {
                        return ResourceCommandState.Enabled;
                    }

                    return ResourceCommandState.Disabled;
                }
            });
        
        return builder;
    }
}