using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries;

[Handler]
public sealed partial class GetTodo : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/todos/{id:guid}",
                (Guid id, Handler handler, CancellationToken cancellationToken)
                    => handler.HandleAsync(new Query(id), cancellationToken))
            .Produces<Todo>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));
    }
    
    public sealed record Query(Guid Id);

    private static async ValueTask<Todo> HandleAsync(Query request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        return todo;
    }
}