namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands.DeleteTodo;

public sealed class DeleteTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDeleteWithOpenApi("/todo/{id:guid}", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(Guid id, ITodoRepository todoRepository,
        CancellationToken cancellationToken)
    {
        var output = await todoRepository.GetByIdAsync(id, cancellationToken);

        if (output == null) return Results.NotFound();

        await todoRepository.DeleteAsync(output, cancellationToken);

        return Results.NoContent();
    }
}