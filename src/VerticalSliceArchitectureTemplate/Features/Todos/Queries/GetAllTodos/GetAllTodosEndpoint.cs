using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries.GetAllTodos;

public sealed class GetAllTodosEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetWithOpenApi<Todo[]>("/todos", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(bool? isCompleted, AppDbContext todoRepository,
        CancellationToken cancellationToken)
    {
        var output = await todoRepository.Todos
            .Where(x => isCompleted == null || x.Completed == isCompleted)
            .ToArrayAsync(cancellationToken);

        return Results.Ok(output);
    }
}