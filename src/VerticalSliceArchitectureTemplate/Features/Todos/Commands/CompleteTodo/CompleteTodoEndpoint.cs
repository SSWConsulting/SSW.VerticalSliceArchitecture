



using Ardalis.Result.AspNetCore;
using VerticalSliceArchitectureTemplate.Common;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands.CompleteTodo;

public sealed class CompleteTodoEndpoint : IEndpoint
{
    public static async Task<IResult> HandleAsync(Guid id, ITodoRepository todoRepository, CancellationToken cancellationToken)
    {
        var todo = await todoRepository.GetByIdAsync(id, cancellationToken);
        
        if (todo == null)
        {
            return Results.NotFound();
        }
        
        var result = todo.Complete();

        if (!result.IsSuccess)
        {
            return result.ToMinimalApiResult();
        }
        
        await todoRepository.UpdateAsync(todo, cancellationToken);

        return Results.NoContent();
    }

    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPostWithUpdatedOpenApi("/todo/{id:guid}/complete", handler: HandleAsync)
            .WithTags(nameof(Todo));
    }
}