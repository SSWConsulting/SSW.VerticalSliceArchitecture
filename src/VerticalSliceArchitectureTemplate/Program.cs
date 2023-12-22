using VerticalSliceArchitectureTemplate;
using VerticalSliceArchitectureTemplate.Features.Todos;
using VerticalSliceArchitectureTemplate.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEfCore();
builder.Services.AddMediatR(configure => configure.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Features
builder.Services.AddTodoFeature();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseProductionExceptionHandler();

app.RegisterEndpoints();

app.Run();