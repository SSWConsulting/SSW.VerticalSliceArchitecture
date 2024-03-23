using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;


[Handler]
public sealed partial class UpdateTodo
{
    public sealed record Command(Guid Id, string Text) : IRequest;

    private static async ValueTask HandleAsync(Command request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        todo.Text = request.Text;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}