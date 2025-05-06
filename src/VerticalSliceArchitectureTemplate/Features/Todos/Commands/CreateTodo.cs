using MediatR;
using VerticalSliceArchitectureTemplate.Common.Extensions;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public static class CreateTodo
{
    public record Request(string Text) : IRequest<Guid>;

    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/todos",
                    async (
                        Request request,
                        ISender sender,
                        CancellationToken cancellationToken) =>
                    {
                        await sender.Send(request, cancellationToken);
                        return TypedResults.Ok();
                    })
                .WithName("CreateTodo")
                // SM: 'With Tags'?
                .ProducesPost();
        }
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.Text)
                .NotEmpty();
        }
    }
    
    internal sealed class Handler(AppDbContext dbContext)
        : IRequestHandler<Request, Guid>
    {
        public async Task<Guid> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            var todo = new Todo
            {
                Text = request.Text
            };

            await dbContext.Todos.AddAsync(todo, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return todo.Id;
        }
    }
}