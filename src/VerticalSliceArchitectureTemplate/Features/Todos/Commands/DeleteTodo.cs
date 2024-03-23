using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

[Handler]
public sealed partial class DeleteTodo
{
    public sealed record Command(Guid Id) : IRequest;

    private static async ValueTask HandleAsync(Command request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        dbContext.Todos.Remove(todo);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}