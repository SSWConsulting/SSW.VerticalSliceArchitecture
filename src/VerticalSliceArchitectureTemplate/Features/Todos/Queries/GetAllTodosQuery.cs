using System.Collections.Immutable;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries;

public sealed record GetAllTodosQuery(bool? IsCompleted = null) : IRequest<IReadOnlyList<Todo>>;

public sealed class GetAllTodosQueryHandler(AppDbContext dbContext) : IRequestHandler<GetAllTodosQuery, IReadOnlyList<Todo>>
{
    public async Task<IReadOnlyList<Todo>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await dbContext.Todos
            .Where(x => request.IsCompleted == null || x.IsCompleted == request.IsCompleted)
            .ToListAsync(cancellationToken);

        return todos.AsReadOnly();
    }
}