using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries;

public sealed record GetTodoQuery(Guid Id) : IRequest<Todo>;

public sealed class GetTodoQueryHandler(AppDbContext dbContext) : IRequestHandler<GetTodoQuery, Todo>
{
    public async Task<Todo> Handle(GetTodoQuery request, CancellationToken cancellationToken)
    {
        var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

        if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

        return todo;
    }
}