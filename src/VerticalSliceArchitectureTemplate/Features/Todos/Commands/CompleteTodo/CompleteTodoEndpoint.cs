using Ardalis.Result.AspNetCore;
using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands.CompleteTodo;

public sealed class CompleteTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPostWithUpdatedOpenApi("/todos/{id:guid}/complete", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(Guid id, AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([id], cancellationToken);

        if (todo == null) return Results.NotFound();

        var result = todo.Complete();

        if (!result.IsSuccess) return result.ToMinimalApiResult();

        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}