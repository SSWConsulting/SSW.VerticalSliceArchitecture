using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceArchitectureTemplate.Features.Todos.Commands;
using VerticalSliceArchitectureTemplate.Features.Todos.Domain;
using VerticalSliceArchitectureTemplate.Features.Todos.Queries;

namespace VerticalSliceArchitectureTemplate.Features.Todos.WebApi;

public class TodoEndpoints : IEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/todos")
            .WithTags(nameof(Todo));

        group.MapPost("",
                async (CreateTodo.Command command, CreateTodo.Handler handler, CancellationToken cancellationToken) =>
                {
                    var id = await handler.HandleAsync(command, cancellationToken);
                    return Results.Created($"/todos/{id}", id);
                })
            .Produces<Todo>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapPut("/{id:guid}",
                async (Guid id, UpdateTodo.Command command, UpdateTodo.Handler handler, CancellationToken cancellationToken) =>
                {
                    await handler.HandleAsync(command with
                    {
                        Id = id // TODO: Remove this duplication
                    }, cancellationToken);
                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapPut("/{id:guid}/complete",
                async ([FromRoute] Guid id, CompleteTodo.Handler handler, CancellationToken cancellationToken) =>
                {
                    await handler.HandleAsync(new CompleteTodo.Command(id), cancellationToken);
                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapDelete("/{id:guid}",
                async ([FromRoute] Guid id, DeleteTodo.Handler handler, CancellationToken cancellationToken) =>
                {
                    await handler.HandleAsync(new DeleteTodo.Command(id), cancellationToken);
                    return Results.NoContent();
                })
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapGet("/{id:guid}",
                (Guid id, GetTodo.Handler handler, CancellationToken cancellationToken)
                    => handler.HandleAsync(new GetTodo.Query(id), cancellationToken))
            .Produces<Todo>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));

        group.MapGet("",
                ([AsParameters] GetAllTodos.Query query, GetAllTodos.Handler handler, CancellationToken cancellationToken)
                    => handler.HandleAsync(query, cancellationToken))
            .Produces<IReadOnlyList<Todo>>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithTags(nameof(Todo));
    }
}