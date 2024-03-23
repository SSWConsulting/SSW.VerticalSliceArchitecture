using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

[Handler]
public sealed partial class UpdateTodo : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/todos/{id:guid}",
                async (Guid id, Command command, Handler handler, CancellationToken cancellationToken) =>
                {
                    await handler.HandleAsync(command with
                    {
                        Id = id // TODO: Remove this duplication
                    }, cancellationToken);
                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));
    }
    
    public sealed record Command(Guid Id, string Text);

    private static async ValueTask HandleAsync(Command request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        todo.Text = request.Text;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}