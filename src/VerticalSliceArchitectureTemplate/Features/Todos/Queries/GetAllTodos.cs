using VerticalSliceArchitectureTemplate.Features.Todos.Domain;
using MediatR;
using VerticalSliceArchitectureTemplate.Common.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries;

public static class GetAllTodos
{
    public record Request(bool? IsCompleted = null) : IRequest<IReadOnlyList<Todo>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/todos",
                    async (ISender sender, CancellationToken cancellationToken) =>
                    {
                        var request = new Request();
                        var response = await sender.Send(request, cancellationToken);
                        return TypedResults.Ok(response);
                    })
                .WithName("GetAllTodos")
                .WithTags("Todos")
                .ProducesGet<IReadOnlyList<Todo>>();
        }
    }
    
    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.IsCompleted)
                .NotEmpty();
        }
    }
    
    // SM: Interface for DbContext? IAppDbContext
    internal sealed class Handler(AppDbContext dbContext)
        : IRequestHandler<Request, IReadOnlyList<Todo>>
    {
        public async Task<IReadOnlyList<Todo>> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            var todos = await dbContext.Todos
                .Where(x => request.IsCompleted == null || x.IsCompleted == request.IsCompleted)
                .ToListAsync(cancellationToken);
    
            return todos.AsReadOnly();
        }
    }
}