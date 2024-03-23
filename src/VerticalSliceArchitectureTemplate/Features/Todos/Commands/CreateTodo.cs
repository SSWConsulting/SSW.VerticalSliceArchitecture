using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

[Handler]
public sealed partial class CreateTodo : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/todos",
                async (Command command, Handler handler, CancellationToken cancellationToken) =>
                {
                    var id = await handler.HandleAsync(command, cancellationToken);
                    return Results.Created($"/todos/{id}", id);
                })
            .Produces<Todo>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));
    }
    
    public sealed record Command(string Text);

    private static async ValueTask<Guid> HandleAsync(Command request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var todo = new Todo
        {
            Text = request.Text
        };

        await dbContext.Todos.AddAsync(todo, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return todo.Id;
    }
}