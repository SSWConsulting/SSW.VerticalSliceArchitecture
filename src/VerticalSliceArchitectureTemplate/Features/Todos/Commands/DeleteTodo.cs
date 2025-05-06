using System.Text.Json.Serialization;
using MediatR;
using VerticalSliceArchitectureTemplate.Common.Extensions;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public static class DeleteTodo
{
    public record Request : IRequest
    {
        [JsonIgnore]
        public Guid Id { get; set; }
    }

    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete("/todos/{id:guid}",
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
                .WithTags("Todos")
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
    
    internal sealed class Handler(AppDbContext dbContext)
        : IRequestHandler<Request>
    {
        public async Task Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            var todo = await dbContext.Todos.FindAsync([request.Id], cancellationToken);

            if (todo == null) throw new NotFoundException(nameof(Todo), request.Id);

            dbContext.Todos.Remove(todo);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}