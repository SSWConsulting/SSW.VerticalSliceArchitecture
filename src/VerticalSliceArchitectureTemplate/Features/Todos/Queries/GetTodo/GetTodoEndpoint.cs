using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries.GetTodo;

public sealed class GetTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetWithOpenApi<Todo>("/todos/{id:guid}", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(Guid id, AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var output = await dbContext.Todos.FindAsync([id], cancellationToken);

        return output == null
            ? Results.NotFound()
            : Results.Ok(output);
    }
}