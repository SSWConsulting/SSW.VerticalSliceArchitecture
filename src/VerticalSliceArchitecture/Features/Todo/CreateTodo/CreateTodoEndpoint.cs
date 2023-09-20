using VerticalSliceArchitecture.Features.Todo.GetTodo;

namespace VerticalSliceArchitecture.Features.Todo.CreateTodo;

public class CreateTodoEndpoint : Endpoint<CreateTodoRequest, Guid>
{
    private readonly TodoRepository _todoRepository;

    public CreateTodoEndpoint(TodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public override void Configure()
    {
        Post("/todo");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateTodoRequest request, CancellationToken cancellationToken)
    {
        var output = new TodoEntity
        {
            Text = request.Text,
            Completed = false
        };

        await _todoRepository.AddAsync(output, cancellationToken);

        await SendCreatedAtAsync<GetTodoEndpoint>(output.Id, output.Id, cancellation: cancellationToken);
    }
}