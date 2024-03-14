using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands.CreateTodo;

public sealed class CreateTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPostWithCreatedOpenApi("/todos", HandleAsync)
            .WithTags(nameof(Todo));
    }

    public static async Task<IResult> HandleAsync(CreateTodoRequest request, AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var output = new Todo
        {
            Text = request.Text
        };
        
        await dbContext.Todos.AddAsync(output, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created("/todos", output.Id);
    }
}