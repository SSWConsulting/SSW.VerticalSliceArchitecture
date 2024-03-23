using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public sealed record CreateTodoCommand(string Text) : IRequest<Guid>;

public sealed class CreateTodoCommandHandler(AppDbContext dbContext) : IRequestHandler<CreateTodoCommand, Guid>
{
    public async Task<Guid> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
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