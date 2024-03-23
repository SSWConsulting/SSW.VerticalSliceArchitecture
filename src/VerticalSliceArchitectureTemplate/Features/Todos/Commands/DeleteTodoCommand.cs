using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public sealed record DeleteTodoCommand(Guid Id) : IRequest;

public sealed class DeleteTodoCommandHandler(AppDbContext dbContext) : IRequestHandler<DeleteTodoCommand>
{
    public async Task Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        dbContext.Todos.Remove(todo);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}