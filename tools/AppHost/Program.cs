using AppHost.Extensions;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Ensure the port doesn't conflict with other docker containers (or remove it altogether)
var sqlServer = builder
    .AddSqlServer("sql-server", port: 1800)
    .WithLifetime(ContainerLifetime.Persistent);

var db = sqlServer
    .AddDatabase("app-db")
    .WithDropDatabaseCommand();

var migrationService = builder.AddProject<MigrationService>("migrations")
    .WithReference(db)
    .WaitFor(sqlServer);

builder
    .AddProject<VerticalSliceArchitectureTemplate>("vertical-slice-architecture")
    .WithEndpoint("https", endpoint => endpoint.IsProxied = false)
    .WithReference(db)
    .WaitFor(migrationService);

builder.Build().Run();
