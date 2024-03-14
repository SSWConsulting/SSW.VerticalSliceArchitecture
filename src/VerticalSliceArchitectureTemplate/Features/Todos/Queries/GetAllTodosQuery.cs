using System.Collections.Immutable;
using VerticalSliceArchitectureTemplate.Features.Todos.Models;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries;

public sealed record GetAllTodosQuery(bool? IsCompleted = null) : IRequest<IImmutableList<Todo>>;

public sealed class GetAllTodosQueryHandler(AppDbContext dbContext) : IRequestHandler<GetAllTodosQuery, IImmutableList<Todo>>
{
    public async Task<IImmutableList<Todo>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await dbContext.Todos
            .Where(x => request.IsCompleted == null || x.IsCompleted == request.IsCompleted)
            .ToArrayAsync(cancellationToken);

        return todos.ToImmutableList();
    }
}