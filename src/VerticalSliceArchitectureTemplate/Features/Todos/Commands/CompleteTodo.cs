using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;


[Handler]
public sealed partial class CompleteTodo
{
    public sealed record Command(Guid Id);
    private static async ValueTask HandleAsync(Command request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        todo.Complete();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}