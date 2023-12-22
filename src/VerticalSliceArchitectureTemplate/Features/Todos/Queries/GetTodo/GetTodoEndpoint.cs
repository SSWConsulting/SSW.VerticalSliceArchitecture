
using VerticalSliceArchitectureTemplate.Common;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries.GetTodo;

public sealed class GetTodoEndpoint : IEndpoint
{
    public static async Task<IResult> HandleAsync(Guid id, ITodoRepository todoRepository, CancellationToken cancellationToken)
    {
        var output = await todoRepository.GetByIdAsync(id, cancellationToken);

        if (output == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(output);
    }

    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetWithOpenApi<Todo>("/todo/{id:guid}", handler: HandleAsync)
            .WithTags(nameof(Todo));
    }
}