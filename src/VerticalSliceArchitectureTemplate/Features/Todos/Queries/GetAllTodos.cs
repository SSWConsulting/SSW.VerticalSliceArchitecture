using VerticalSliceArchitectureTemplate.Features.Todos.Domain;
using MediatR;
using VerticalSliceArchitectureTemplate.Common.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries;

public static class GetAllTodos
{
    public record Request(bool? IsCompleted = null) : IRequest<ErrorOr<IReadOnlyList<Todo>>>;
    
    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/todos",
                    async (ISender sender, CancellationToken cancellationToken, bool? isCompleted = null) =>
                    {
                        var request = new Request(isCompleted);
                        var response = await sender.Send(request, cancellationToken);
                        return TypedResults.Ok(response);
                    })
                .WithName("GetAllTodos")
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
    
    internal sealed class Handler : IRequestHandler<Request, ErrorOr<IReadOnlyList<Todo>>>
    {
        private readonly AppDbContext _dbContext;

        public Handler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ErrorOr<IReadOnlyList<Todo>>> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            var todos = await _dbContext.Todos
                .Where(x => request.IsCompleted == null || x.IsCompleted == request.IsCompleted)
                .ToListAsync(cancellationToken);
    
            return todos.AsReadOnly();
        }
    }
}