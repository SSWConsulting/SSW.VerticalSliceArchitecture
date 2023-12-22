namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands.UpdateTodo;

public sealed class UpdateTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPutWithOpenApi("/todos/{id:guid}", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(Guid id, UpdateTodoRequest input, ITodoRepository todoRepository,
        CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(id, cancellationToken);

        if (todo == null) return Results.NotFound();

        todo.Text = input.Text;

        await todoRepository.UpdateAsync(todo, cancellationToken);

        return Results.Ok(todo);
    }
}