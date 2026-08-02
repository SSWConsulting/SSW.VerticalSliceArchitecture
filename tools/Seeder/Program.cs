using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Seeder;
using Seeder.Initializers;
using SSW.VerticalSliceArchitecture.Common.Interfaces;
using SSW.VerticalSliceArchitecture.Common.Persistence;
using SSW.VerticalSliceArchitecture.Common.Persistence.Interceptors;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.Services.AddScoped<ApplicationDbContextInitializer>();
builder.Services.AddSingleton(TimeProvider.System);

// Singleton, not scoped: AddSqlServerDbContext registers a *pooled* DbContext, so its options
// are built once against the root provider, which cannot resolve scoped services. Both types
// are safe as singletons here — the interceptor holds no per-operation state, and the seeder
// runs as one process under one fixed identity.
builder.Services.AddSingleton<EntitySaveChangesInterceptor>();
builder.Services.AddSingleton<ICurrentUserService, SeederUserService>();

builder.AddSqlServerDbContext<ApplicationDbContext>("AppDb");

// AddSqlServerDbContext's own options callback hands back no service provider, which is what
// pushed the original registration into calling builder.Services.BuildServiceProvider(). That
// builds a second, detached container, so the interceptor it resolved belonged to a different
// object graph than the one the app runs on. ConfigureDbContext supplies the real provider.
builder.Services.ConfigureDbContext<ApplicationDbContext>((serviceProvider, options) =>
    options.AddInterceptors(serviceProvider.GetRequiredService<EntitySaveChangesInterceptor>()));

var host = builder.Build();

await host.RunAsync();
