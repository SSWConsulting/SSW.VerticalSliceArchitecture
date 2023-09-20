namespace VerticalSliceArchitecture.Features.Todo.DeleteTodo;

public class DeleteTodoEndpoint : Endpoint<DeleteTodoRequest>
{
    private readonly TodoRepository _todoRepository;

    public DeleteTodoEndpoint(TodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public override void Configure()
    {
        Delete("/todo/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteTodoRequest request, CancellationToken cancellationToken)
    {
        var output = await _todoRepository.GetByIdAsync(request.Id, cancellationToken);

        if (output == null)
        {
            await SendNotFoundAsync(cancellation: cancellationToken);
            return;
        }

        await _todoRepository.DeleteAsync(output, cancellationToken);

        await SendNoContentAsync(cancellation: cancellationToken);
    }
}