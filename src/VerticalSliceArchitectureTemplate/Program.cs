using System.Reflection;
using VerticalSliceArchitectureTemplate.Host;
using VerticalSliceArchitectureTemplate.Common.Extensions;
using VerticalSliceArchitectureTemplate.Common.HealthChecks;

var appAssembly = Assembly.GetExecutingAssembly();
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCustomProblemDetails();

builder.Services.AddSwaggerGen(options =>
{ 
    options.CustomSchemaIds(x => x.FullName?.Replace("+", ".", StringComparison.Ordinal));
});

builder.AddApplication();
builder.Services.AddEndpointsApiExplorer();

builder.AddInfrastructure();

builder.Services.ConfigureFeatures(builder.Configuration, appAssembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.RegisterEndpoints(appAssembly);
app.MapOpenApi();
app.UseHealthChecks();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseEventualConsistencyMiddleware();

app.MapDefaultEndpoints();
app.UseExceptionHandler();

app.Run();

public partial class Program;
