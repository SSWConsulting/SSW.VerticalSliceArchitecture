namespace VerticalSliceArchitecture.Features.Todos.Commands.UpdateTodo;

public sealed class UpdateTodoEndpoint : IEndpoint
{
    public static async Task<IResult> HandleAsync(Guid id, UpdateTodoRequest input, ITodoRepository todoRepository, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(id, cancellationToken);

        if (todo == null)
        {
            return Results.NotFound();
        }

        todo.Text = input.Text;

        await todoRepository.UpdateAsync(todo, cancellationToken);

        return Results.Ok(todo);
    }

    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPutWithOpenApi("/todo/{id:guid}", handler: HandleAsync)
            .WithTags(nameof(Todo));
    }
}