using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries;

[Handler]
public sealed partial class GetTodo
{
    public sealed record Query(Guid Id);

    private static async ValueTask<Todo> HandleAsync(Query request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        return todo;
    }
}