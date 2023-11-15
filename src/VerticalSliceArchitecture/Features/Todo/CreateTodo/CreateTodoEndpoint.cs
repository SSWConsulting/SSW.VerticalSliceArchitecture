using VerticalSliceArchitecture.Features.Todo.GetTodo;

namespace VerticalSliceArchitecture.Features.Todo.CreateTodo;

public class CreateTodoEndpoint : IEndpoint
{
    public static async Task<IResult> HandleAsync(CreateTodoRequest request, ITodoRepository todoRepository, CancellationToken cancellationToken)
    {
        var output = new TodoEntity
        {
            Text = request.Text,
            Completed = false
        };

        await todoRepository.AddAsync(output, cancellationToken);
        
        return Results.Created();
    }

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/todo", handler: HandleAsync);
    }
}