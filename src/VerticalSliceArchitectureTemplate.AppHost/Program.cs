var builder = DistributedApplication.CreateBuilder(args);

var appDb = builder
    .AddSqlServer("db")
    .AddDatabase("appdb");

builder.AddProject<Projects.VerticalSliceArchitectureTemplate>("webapi")
    .WithReference(appDb);

builder.Build().Run();
