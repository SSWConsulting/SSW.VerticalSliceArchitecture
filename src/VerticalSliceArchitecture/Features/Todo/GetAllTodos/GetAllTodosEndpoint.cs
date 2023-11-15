namespace VerticalSliceArchitecture.Features.Todo.GetAllTodos;

public class GetAllTodosEndpoint : IEndpoint
{
    public static async Task<IResult> HandleAsync(ITodoRepository todoRepository, CancellationToken cancellationToken)
    {
        var output = await todoRepository.GetAllAsync(cancellationToken);

        return Results.Ok(output);
    }

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/todo", handler: HandleAsync);
    }
}