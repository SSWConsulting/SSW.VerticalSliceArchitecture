using Microsoft.AspNetCore.Mvc;

namespace VerticalSliceArchitecture.Features.Todo.UpdateTodo;

public class UpdateTodoEndpoint : IEndpoint
{
    public async Task<IResult> HandleAsync(Guid id, TodoEntity input, ITodoRepository todoRepository, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(input.Id, cancellationToken);

        if (todo == null)
        {
            return Results.NotFound();
        }

        todo.Text = input.Text;
        todo.Completed = input.Completed;

        await todoRepository.UpdateAsync(todo, cancellationToken);

        return Results.Ok(todo);
    }

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/todo/{id}", handler: HandleAsync);
    }
}