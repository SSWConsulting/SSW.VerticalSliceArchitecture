
namespace VerticalSliceArchitecture.Features.Todo.GetTodo;

public class GetTodoEndpoint : IEndpoint
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

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/todo/{id}", handler: HandleAsync);
    }
}