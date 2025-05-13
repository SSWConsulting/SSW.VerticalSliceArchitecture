using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace MigrationService.Initializers;

public abstract class DbContextInitializerBase<T> where T : DbContext
{
    protected T DbContext { get; }

    // public constructor needed for DI
    internal DbContextInitializerBase(T dbContext)
    {
        DbContext = dbContext;
    }

    public async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
    {
        var dbCreator = DbContext.GetService<IRelationalDatabaseCreator>();
        var strategy = DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Create the database if it does not exist.
            // Do this first so there is then a database to start a transaction against.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }

    public async Task CreateSchemaAsync(bool useMigrations, CancellationToken cancellationToken)
    {
        var strategy = DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (useMigrations)
            {
                await DbContext.Database.MigrateAsync(cancellationToken);
            }
            else
            {
                var dbCreator = DbContext.GetService<IRelationalDatabaseCreator>();
                if (!await dbCreator.HasTablesAsync(cancellationToken))
                {
                    await dbCreator.CreateTablesAsync(cancellationToken);
                }
            }
        });
    }
}