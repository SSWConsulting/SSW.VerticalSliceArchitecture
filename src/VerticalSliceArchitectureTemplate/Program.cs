using System.Reflection;
using VerticalSliceArchitectureTemplate.Kernel;

var appAssembly = Assembly.GetExecutingAssembly();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEfCore();
builder.Services.AddMediatR(configure => configure.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<ExceptionHandler.KnownExceptionsHandler>();

builder.Services.ConfigureModules(appAssembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseProductionExceptionHandler();

app.RegisterEndpoints(appAssembly);

app.Run();