using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationService.Initializers;
using System.Diagnostics;

namespace MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            var sw = Stopwatch.StartNew();
            using var scope = serviceProvider.CreateScope();
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

            var warehouseInitializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
            await warehouseInitializer.EnsureDatabaseAsync(stoppingToken);
            await warehouseInitializer.CreateSchemaAsync(true, stoppingToken);

            if (environment.IsDevelopment())
            {
                await warehouseInitializer.SeedDataAsync(stoppingToken);
            }

            sw.Stop();
            logger.LogInformation("DB creation and seeding took {ElapsedTime}", sw.Elapsed);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }
}