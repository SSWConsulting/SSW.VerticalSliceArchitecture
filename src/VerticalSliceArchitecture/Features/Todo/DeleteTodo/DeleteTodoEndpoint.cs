namespace VerticalSliceArchitecture.Features.Todo.DeleteTodo;

public class DeleteTodoEndpoint : IEndpoint
{
    public static async Task<IResult> HandleAsync(Guid id, ITodoRepository todoRepository, CancellationToken cancellationToken)
    {
        var output = await todoRepository.GetByIdAsync(id, cancellationToken);

        if (output == null)
        {
            return Results.NotFound();
        }

        await todoRepository.DeleteAsync(output, cancellationToken);

        return Results.NoContent();
    }

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/todo/{id}", HandleAsync);
    }
}