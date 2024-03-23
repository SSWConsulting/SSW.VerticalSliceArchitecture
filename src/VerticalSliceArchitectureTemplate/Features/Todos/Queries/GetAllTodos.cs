using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries;

[MapGet("/todos")]
[Handler]
public sealed partial class GetAllTodos
{
    internal static void CustomizeEndpoint(IEndpointConventionBuilder endpoint)
        => endpoint
            .WithTags(nameof(Todo));
    
    public sealed record Query(bool? IsCompleted = null);

    private static async ValueTask<List<Todo>> HandleAsync(Query request, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var todos = await dbContext.Todos
            .Where(x => request.IsCompleted == null || x.IsCompleted == request.IsCompleted)
            .ToListAsync(cancellationToken);
        
        return todos;
    }
}