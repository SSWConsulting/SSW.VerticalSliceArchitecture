using AppHost.Commands;
using Azure.Provisioning;
using Azure.Provisioning.AppService;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Configure the Azure App Service environment
builder.AddAzureAppServiceEnvironment("plan").ConfigureInfrastructure(infra =>
{
    var plan = infra.GetProvisionableResources()
        .OfType<AppServicePlan>()
        .Single();

    plan.Sku = new AppServiceSkuDescription
    {
        Name = "B1", // Basic tier, 1 core
    };
});

var sqlServer = builder
    .AddAzureSqlServer("sql")
    .RunAsContainer(container =>
    {
        // Configure SQL Server to run locally as a container
        container.WithLifetime(ContainerLifetime.Persistent);

        // SQL Server 2025. Runs locally on macOS via OrbStack (Docker Desktop on Apple Silicon may not work).
        container.WithImage("mssql/server:2025-latest");

        // Group under one "SSW-VSA" project in Docker Desktop / OrbStack (cosmetic only)
        container.InDockerProject();

        // If desired, set SQL Server Port to a constant value
        //container.WithHostPort(1800);
    });

var db = sqlServer
    .AddDatabase("AppDb", "app-db")
    .WithDropDatabaseCommand();

var api = builder
    .AddProject<WebApi>("api")
    .WithExternalHttpEndpoints()
    .WithReference(db);

// Migrations live in the WebApi project (Common/Persistence/Migrations) alongside the only
// DbContext in that assembly, so neither WithMigrationsProject<T>() nor an explicit context
// name is needed. On publish this emits a migration bundle under efmigrations/ rather than
// running anything — applying it is a deployment step (see the README).
var migrations = api.AddEFMigrations("migrations")
    .WithReference(db)
    .WaitFor(sqlServer)
    .RunDatabaseUpdateOnStart()
    .PublishAsMigrationBundle();

if (builder.ExecutionContext.IsRunMode)
{
    // Seeding is dev-only, and this guard is what enforces it — the resource is never
    // added in publish mode, so Bogus data has no path to a deployed environment.
    var seeder = builder.AddProject<Seeder>("seeder")
        .WithReference(db)
        .WaitForCompletion(migrations);

    api.WaitForCompletion(seeder);
}
else
{
    api.WaitForCompletion(migrations);
}

// Configure Application Insights and Log Analytics only if in publish mode
// When running locally, use Aspire Dashboard instead
if (builder.ExecutionContext.IsPublishMode)
{
    var logAnalytics = builder.AddAzureLogAnalyticsWorkspace("log-analytics");
    var insights = builder.AddAzureApplicationInsights("insights", logAnalytics);
    api.WithReference(insights);
}

builder.Build().Run();