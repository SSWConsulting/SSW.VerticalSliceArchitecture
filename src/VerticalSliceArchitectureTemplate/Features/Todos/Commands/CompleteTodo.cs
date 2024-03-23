using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;


[Handler]
public sealed partial class CompleteTodo : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/todos/{id:guid}/complete",
                async ([FromRoute] Guid id, Handler handler, CancellationToken cancellationToken) =>
                {
                    await handler.HandleAsync(new Command(id), cancellationToken);
                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));
    }
    public sealed record Command(Guid Id);
    private static async ValueTask HandleAsync(Command request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        todo.Complete();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}