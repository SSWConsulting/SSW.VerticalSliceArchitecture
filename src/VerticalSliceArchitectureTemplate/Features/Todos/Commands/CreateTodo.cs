using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

[Handler]
public sealed partial class CreateTodo
{
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