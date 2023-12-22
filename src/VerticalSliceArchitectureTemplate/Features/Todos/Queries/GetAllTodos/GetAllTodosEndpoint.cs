namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries.GetAllTodos;

public sealed class GetAllTodosEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetWithOpenApi<Todo[]>("/todo", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(bool? isCompleted, ITodoRepository todoRepository,
        CancellationToken cancellationToken)
    {
        var output = await todoRepository.GetAllAsync(isCompleted, cancellationToken);

        return Results.Ok(output);
    }
}