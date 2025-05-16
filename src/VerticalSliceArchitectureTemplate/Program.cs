using System.Reflection;
using VerticalSliceArchitectureTemplate.Host;
using VerticalSliceArchitectureTemplate.Common.Extensions;

var appAssembly = Assembly.GetExecutingAssembly();
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCustomProblemDetails();

builder.Services.AddSwaggerGen(options =>
{ 
    options.CustomSchemaIds(x => x.FullName?.Replace("+", ".", StringComparison.Ordinal));
});

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.RegisterEndpoints(appAssembly);
app.UseEventualConsistencyMiddleware();

app.MapDefaultEndpoints();
app.UseExceptionHandler();

app.Run();

public partial class Program;
