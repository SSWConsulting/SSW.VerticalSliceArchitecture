using System.Text.Json.Serialization;
using MediatR;
using VerticalSliceArchitectureTemplate.Common.Extensions;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public static class UpdateTodo
{
    public record Request(String Text) : IRequest
    {
        [JsonIgnore]
        public Guid Id { get; set; }
    }

    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPut("/todos/{id:guid}",
                    async (Guid id, [FromBody] Request request, ISender sender, CancellationToken cancellationToken) =>
                    {
                        request.Id = id;
                        await sender.Send(request, cancellationToken);
                        return Results.NoContent();
                    })
                .WithName("UpdateTodo")
                // SM: 'With Tags'?
                .ProducesPut();
        }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Id)
                .NotEmpty();
            
            RuleFor(r => r.Text)
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

            todo.Text = request.Text;

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}