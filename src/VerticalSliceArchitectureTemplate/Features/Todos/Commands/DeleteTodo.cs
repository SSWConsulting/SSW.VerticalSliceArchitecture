using System.Text.Json.Serialization;
using MediatR;
using VerticalSliceArchitectureTemplate.Common.Domain.Todos;
using VerticalSliceArchitectureTemplate.Common.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public static class DeleteTodo
{
    public record Request : IRequest<ErrorOr<Success>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }
    }

    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(TodoFeature.FeatureName)
                .MapDelete("/{id:guid}",
                    async (
                        Guid id,
                        ISender sender,
                        CancellationToken cancellationToken) =>
                    {
                        var request = new Request { Id = id };
                        await sender.Send(request, cancellationToken);
                        return Results.NoContent();
                    })
                .WithName("DeleteTodo")
                .ProducesDelete();
        }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Id)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler : IRequestHandler<Request, ErrorOr<Success>>
    {
        private readonly AppDbContext _dbContext;

        public Handler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ErrorOr<Success>> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            var todo = await _dbContext.Todos.FindAsync([request.Id], cancellationToken);

            if (todo == null) return TodoErrors.NotFound;

            _dbContext.Todos.Remove(todo);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Success();
        }
    }
}