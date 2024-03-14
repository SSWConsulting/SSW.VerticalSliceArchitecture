using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands.UpdateTodo;

public sealed class UpdateTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPutWithOpenApi("/todos/{id:guid}", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(Guid id, UpdateTodoRequest input, AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([id], cancellationToken);

        if (todo == null) return Results.NotFound();

        todo.Text = input.Text;
        
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(todo);
    }
}