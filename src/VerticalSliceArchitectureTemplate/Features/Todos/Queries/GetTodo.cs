using System.Text.Json.Serialization;
using MediatR;
using VerticalSliceArchitectureTemplate.Common.Extensions;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Queries;

public static class GetTodo
{
    public record Request : IRequest<Todo>
    {
        [JsonIgnore]
        public Guid Id { get; set; }
    }

    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/todos/{id:guid}",
                    (Guid id, ISender sender, CancellationToken cancellationToken) =>
                    {
                        var request = new Request { Id = id };
                        return sender.Send(request, cancellationToken);
                    })
                .WithName("GetTodo")
                .WithTags("Todos")
                .ProducesGet<Todo>();
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

    internal sealed class Handler(AppDbContext dbContext)
        : IRequestHandler<Request, Todo>
    {
        public async Task<Todo> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

            if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

            return todo;
        }
    }
}