namespace VerticalSliceArchitecture.Features.Todo.GetAllTodos;

public class GetAllTodosEndpoint : EndpointWithoutRequest<IEnumerable<TodoEntity>>
{
    private readonly ITodoRepository _todoRepository;

    public GetAllTodosEndpoint(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public override void Configure()
    {
        Get("/todo");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var output = await _todoRepository.GetAllAsync(cancellationToken);

        await SendAsync(output, cancellation: cancellationToken);
    }
}