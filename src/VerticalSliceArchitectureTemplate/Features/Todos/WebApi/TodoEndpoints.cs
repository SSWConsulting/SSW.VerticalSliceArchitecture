using System.Collections.Immutable;
using VerticalSliceArchitectureTemplate.Features.Todos.Commands;
using VerticalSliceArchitectureTemplate.Features.Todos.Models;
using VerticalSliceArchitectureTemplate.Features.Todos.Queries;

namespace VerticalSliceArchitectureTemplate.Features.Todos.WebApi;

public class TodoEndpoints : IEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/todos")
            .WithTags(nameof(Todo));

        group.MapPost("",
                async (CreateTodoCommand command, ISender sender, CancellationToken cancellationToken) =>
                {
                    var id = await sender.Send(command, cancellationToken);
                    return Results.Created($"/todos/{id}", id);
                })
            .Produces<Todo>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapPut("/{id:guid}",
                async (Guid id, UpdateTodoCommand command, ISender sender, CancellationToken cancellationToken) =>
                {
                    await sender.Send(command with { Id = id }, cancellationToken);
                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapPut("/{id:guid}/complete",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    await sender.Send(new CompleteTodoCommand(id), cancellationToken);
                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapDelete("/{id:guid}",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    await sender.Send(new DeleteTodoCommand(id), cancellationToken);
                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapGet("/{id:guid}",
                (Guid id, ISender sender, CancellationToken cancellationToken)
                    => sender.Send(new GetTodoQuery(id), cancellationToken))
            .Produces<Todo>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapGet("",
                (bool? isCompleted, ISender sender, CancellationToken cancellationToken)
                    => sender.Send(new GetAllTodosQuery(isCompleted), cancellationToken))
            .Produces<IImmutableList<Todo>>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));
    }
}