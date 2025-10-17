using System.Reflection;
using FastEndpoints.Swagger;
using SSW.VerticalSliceArchitecture.Common.FastEndpoints;
using SSW.VerticalSliceArchitecture.Host.Extensions;
using SSW.VerticalSliceArchitecture.Host;

var appAssembly = Assembly.GetExecutingAssembly();
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCustomProblemDetails();

builder.AddWebApi();
builder.AddApplication();
builder.AddInfrastructure();

builder.Services.ConfigureFeatures(builder.Configuration, appAssembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Add back in to rest out Scalar
// app.MapOpenApi();
// app.MapCustomScalarApiReference();

app.UseHttpsRedirection();
app.UseStaticFiles();

// Use FastEndpoints alongside minimal APIs
app.UseFastEndpoints(config =>
{
    config.Endpoints.RoutePrefix = "api";
    config.Endpoints.Configurator = ep =>
    {
        // Add global pre-processors
        ep.PreProcessor<LoggingPreProcessor>(Order.Before);
        ep.PreProcessor<PerformancePreProcessor>(Order.Before);
        
        // Add global post-processors
        ep.PostProcessor<PerformancePostProcessor>(Order.After);
        ep.PostProcessor<EventualConsistencyPostProcessor>(Order.After);
    };
    config.Errors.UseProblemDetails();
});

app.UseSwaggerGen(uiConfig: c =>
{
//     c.DefaultModelsExpandDepth = 1;
//     c.DefaultModelExpandDepth = 1;
//     // Set application/json as the default content type
//     c.AdditionalSettings["contentType"] = "application/json";
});

// app.RegisterEndpoints(appAssembly);
// app.MapFastEndpoints();
// app.UseEventualConsistencyMiddleware();

app.MapDefaultEndpoints();
app.UseExceptionHandler();

app.Run();

namespace SSW.VerticalSliceArchitecture
{
    public partial class Program;
}