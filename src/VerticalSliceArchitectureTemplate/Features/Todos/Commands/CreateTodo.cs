using MediatR;
using VerticalSliceArchitectureTemplate.Common.Extensions;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain;
using TodoId = VerticalSliceArchitectureTemplate.Features.Todos.Domain.TodoId;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public static class CreateTodo
{
    public record Request(string Text) : IRequest<ErrorOr<TodoId>>;

    public class Endpoint : IEndpoint
    {
        public static void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints
                .MapApiGroup(TodoFeature.FeatureName)
                .MapPost("/",
                    async (
                        Request request,
                        ISender sender,
                        CancellationToken cancellationToken) =>
                    {
                        await sender.Send(request, cancellationToken);
                        return TypedResults.Ok();
                    })
                .WithName("CreateTodo")
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

    internal sealed class Handler : IRequestHandler<Request, ErrorOr<TodoId>>
    {
        private readonly AppDbContext _dbContext;

        public Handler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ErrorOr<TodoId>> Handle(
            Request request,
            CancellationToken cancellationToken)
        {
            var todo = new Todo
            {
                Text = request.Text
            };

            await _dbContext.Todos.AddAsync(todo, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return todo.Id;
        }
    }
}