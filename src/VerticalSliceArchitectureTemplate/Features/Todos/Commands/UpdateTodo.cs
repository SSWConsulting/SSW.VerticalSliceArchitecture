﻿using System.Text.Json.Serialization;
using MediatR;
using VerticalSliceArchitectureTemplate.Common.Domain.Todos;
using VerticalSliceArchitectureTemplate.Common.Extensions;

namespace VerticalSliceArchitectureTemplate.Features.Todos.Commands;

public static class UpdateTodo
{
    public record Request(String Text) : IRequest<ErrorOr<Success>>
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
                .MapPut("/{id:guid}",
                    async (Guid id, [FromBody] Request request, ISender sender, CancellationToken cancellationToken) =>
                    {
                        request.Id = id;
                        await sender.Send(request, cancellationToken);
                        return Results.NoContent();
                    })
                .WithName("UpdateTodo")
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

            todo.Text = request.Text;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Success();
        }
    }
}