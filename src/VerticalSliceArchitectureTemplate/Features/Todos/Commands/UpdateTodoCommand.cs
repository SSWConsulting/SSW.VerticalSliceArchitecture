using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public sealed record UpdateTodoCommand(Guid Id, string Text) : IRequest;

public sealed class UpdateTodoCommandHandler(AppDbContext dbContext) : IRequestHandler<UpdateTodoCommand>
{
    public async Task Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        todo.Text = request.Text;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}