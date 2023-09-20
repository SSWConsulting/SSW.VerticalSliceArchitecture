namespace VerticalSliceArchitecture.Features.Todo.UpdateTodo;

public class UpdateTodoEndpoint : Endpoint<TodoEntity>
{
    private readonly TodoRepository _todoRepository;

    public UpdateTodoEndpoint(TodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public override void Configure()
    {
        Put("/todo/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(TodoEntity input, CancellationToken cancellationToken)
    {
        var todo = await _todoRepository.GetByIdAsync(input.Id, cancellationToken);

        if (todo == null)
        {
            await SendNotFoundAsync(cancellation: cancellationToken);
            return;
        }

        todo.Text = input.Text;
        todo.Completed = input.Completed;

        await _todoRepository.UpdateAsync(todo, cancellationToken);

        await SendAsync(todo, cancellation: cancellationToken);
    }
}