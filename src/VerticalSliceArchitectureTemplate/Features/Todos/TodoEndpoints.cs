using SSW.CleanArchitecture.WebApi.Extensions;
using VerticalSliceArchitectureTemplate.Features.Todos.Commands;
using VerticalSliceArchitectureTemplate.Features.Todos.Queries;

namespace VerticalSliceArchitectureTemplate.Features.Todos;

public class TodoEndpoints : IEndpoints
{
    public static void MapEndpoints(WebApplication app)
    {
        var group = app.MapApiGroup("todos");

        // SM: I reckon we could do this with reflection as well
        GetAllTodos.Endpoint.MapEndpoint(group);
        GetTodo.Endpoint.MapEndpoint(group);
        CompleteTodo.Endpoint.MapEndpoint(group);
        CreateTodo.Endpoint.MapEndpoint(group);
        DeleteTodo.Endpoint.MapEndpoint(group);
        UpdateTodo.Endpoint.MapEndpoint(group);
    }
}