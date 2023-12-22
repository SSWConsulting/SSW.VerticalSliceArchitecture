namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries.GetTodo;

public sealed class GetTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetWithOpenApi<Todo>("/todo/{id:guid}", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(Guid id, ITodoRepository todoRepository,
        CancellationToken cancellationToken)
    {
        var output = await todoRepository.GetByIdAsync(id, cancellationToken);

        return output == null
            ? Results.NotFound()
            : Results.Ok(output);
    }
}