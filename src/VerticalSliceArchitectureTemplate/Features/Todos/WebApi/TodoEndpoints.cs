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

        group.MapPostWithOpenApi(string.Empty,
            async (CreateTodoCommand command, ISender sender, CancellationToken cancellationToken) =>
            {
                var id = await sender.Send(command, cancellationToken);
                return Results.Created($"/todos/{id}", id);
            });

        group.MapPutWithOpenApi("/{id:guid}",
                async (Guid id, UpdateTodoCommand command, ISender sender, CancellationToken cancellationToken) =>
                {
                    await sender.Send(command with { Id = id }, cancellationToken);
                    return Results.NoContent();
                })
            .WithTags(nameof(Todo));

        group.MapPutWithOpenApi("/{id:guid}/complete",
                async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                {
                    await sender.Send(new CompleteTodoCommand(id), cancellationToken);
                    return Results.NoContent();
                })
            .WithTags(nameof(Todo));

        group.MapDeleteWithOpenApi("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteTodoCommand(id), cancellationToken);
                return Results.NoContent();
            });

        group.MapGetWithOpenApi<Todo>("/{id:guid}",
            (Guid id, ISender sender, CancellationToken cancellationToken)
                => sender.Send(new GetTodoQuery(id), cancellationToken));

        group.MapGetWithOpenApi<IImmutableList<Todo>>(string.Empty,
            (bool? isCompleted, ISender sender, CancellationToken cancellationToken)
                => sender.Send(new GetAllTodosQuery(isCompleted), cancellationToken));
    }
}