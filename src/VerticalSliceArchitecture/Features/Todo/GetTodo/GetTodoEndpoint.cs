
namespace VerticalSliceArchitecture.Features.Todo.GetTodo;

public class GetTodoEndpoint : Endpoint<GetTodoRequest, TodoEntity>
{
    private readonly ITodoRepository _todoRepository;

    public GetTodoEndpoint(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public override void Configure()
    {
        Get("/todo/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetTodoRequest request, CancellationToken cancellationToken)
    {
        var output = await _todoRepository.GetByIdAsync(request.Id, cancellationToken);

        if (output == null)
        {
            await SendNotFoundAsync(cancellation: cancellationToken);
            return;
        }

        await SendAsync(output, cancellation: cancellationToken);
    }
}