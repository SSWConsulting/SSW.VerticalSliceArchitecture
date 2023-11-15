using VerticalSliceArchitecture;
using VerticalSliceArchitecture.Features.Todo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEfCore();

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

app.RegisterEndpoints();

app.Run();