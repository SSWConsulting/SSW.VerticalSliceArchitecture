using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public sealed record CompleteTodoCommand(Guid Id) : IRequest;

public sealed class CompleteTodoCommandHandler(AppDbContext dbContext) : IRequestHandler<CompleteTodoCommand>
{
    public async Task Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        todo.Complete();

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}