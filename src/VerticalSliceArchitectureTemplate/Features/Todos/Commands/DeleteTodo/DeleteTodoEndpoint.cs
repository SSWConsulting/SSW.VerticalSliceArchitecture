using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands.DeleteTodo;

public sealed class DeleteTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDeleteWithOpenApi("/todos/{id:guid}", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(Guid id, AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var output = await dbContext.Todos.FindAsync([id], cancellationToken);

        if (output == null) return Results.NotFound();
        
        dbContext.Todos.Remove(output);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}