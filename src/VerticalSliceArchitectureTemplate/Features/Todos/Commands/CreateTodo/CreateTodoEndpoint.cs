using Ardalis.Result.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using VerticalSliceArchitectureTemplate.Common;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands.CreateTodo;

public sealed class CreateTodoEndpoint : IEndpoint
{
    public static async Task<IResult> HandleAsync(CreateTodoRequest request, ITodoRepository todoRepository, CancellationToken cancellationToken)
    {
        var output = new Todo
        {
            Text = request.Text
        };

        await todoRepository.AddAsync(output, cancellationToken);

        return Results.Created("/todo", output.Id);
    }

    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPostWithCreatedOpenApi("/todo", handler: HandleAsync)
            .WithTags(nameof(Todo));
    }
}