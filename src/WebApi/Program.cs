using System.Reflection;
using SSW.VerticalSliceArchitecture.Host;
using SSW.VerticalSliceArchitecture.Host.Extensions;

var appAssembly = Assembly.GetExecutingAssembly();
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

app.UseHttpsRedirection();
app.UseWebSockets();

app.UseEventualConsistencyMiddleware();

// The GraphQL endpoint, and in development the Nitro IDE, both live at /graphql.
app.MapGraphQL();

app.MapDefaultEndpoints();

// Run(), plus the schema commands: `dotnet run -- schema export` writes the SDL.
app.RunWithGraphQLCommands(args);

namespace SSW.VerticalSliceArchitecture
{
    public partial class Program;
}
