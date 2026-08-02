# Query slice templates

Placeholders as in [command-slice.md](command-slice.md).

A query reads and projects. It never mutates, never calls `SaveChangesAsync`, and never loads a full aggregate when a projection will do — projecting in the `Select` keeps the SQL narrow and skips change tracking entirely.

---

## List query — no request

Three files: endpoint, response, summary. No request record and no validator, because there's nothing to bind or validate. This is `GetAllHeroes`.

### `{UseCase}Endpoint.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}Endpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<{UseCase}Response>
{
    public override void Configure()
    {
        Get("/");
        Group<{Feature}Group>();
        Description(x => x.WithName("{UseCase}"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var {entities} = await dbContext.{Entities}
            .Select(x => new {UseCase}Response.{Entity}Dto(
                x.Id.Value,
                x.Name,
                x.{Children}.Select(c => new {UseCase}Response.{Child}Dto(c.Name, c.Value)).ToList()))
            .ToListAsync(ct);

        await Send.OkAsync(new {UseCase}Response({entities}), ct);
    }
}
```

`EndpointWithoutRequest<TResponse>` is the no-input shape, and its `HandleAsync` takes only the cancellation token.

Project straight into the response DTO inside the `Select`. That means no `Include`, no spec, and no tracked entities — EF translates the whole thing to one query returning only the columns the response needs. Loading entities and mapping afterwards fetches columns nobody reads and pays for change tracking the query will never use.

`x.Id.Value` unwraps the Vogen ID. The API contract speaks `Guid`; strongly typed IDs stay inside the domain.

### `{UseCase}Response.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public record {UseCase}Response(List<{UseCase}Response.{Entity}Dto> {Entities})
{
    public record {Entity}Dto(
        Guid Id,
        string Name,
        IReadOnlyList<{Child}Dto> {Children});

    public record {Child}Dto(string Name, int Value);
}
```

Wrapping the list in a response record rather than returning a bare array leaves room to add paging metadata later without breaking the contract.

### `{UseCase}Summary.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}Summary : Summary<{UseCase}Endpoint>
{
    public {UseCase}Summary()
    {
        Summary = "Get all {entities}";
        Description = "Retrieves a list of all {entities}.";

        // Response example
        Response(200, "{Entities} retrieved successfully",
            example: new {UseCase}Response(
            [
                new {UseCase}Response.{Entity}Dto(
                    Id: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    Name: "Peter Parker",
                    {Children}:
                    [
                        new {UseCase}Response.{Child}Dto("Web Slinging", 3)
                    ])
            ]));
    }
}
```

`Response(200, ..., example: ...)` documents the output shape, where a command's summary uses `ExampleRequest` for the input.

---

## Single-item query — route parameter

Five files, because there's now something to bind and validate. This is `GetTeam`.

### `{UseCase}Endpoint.cs`

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}Endpoint(ApplicationDbContext dbContext)
    : Endpoint<{UseCase}Request, {UseCase}Response>
{
    public override void Configure()
    {
        Get("/{{entity}Id}");
        Group<{Feature}Group>();
        Description(x => x
            .WithName("{UseCase}")
            .Produces(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync({UseCase}Request req, CancellationToken ct)
    {
        var {entity}Id = {Entity}Id.From(req.{Entity}Id);

        var {entity} = await dbContext.{Entities}
            .Where(x => x.Id == {entity}Id)
            .Select(x => new {UseCase}Response(
                x.Id.Value,
                x.Name,
                x.{Children}.Select(c => new {UseCase}Response.{Child}Dto(c.Name, c.Value))))
            .FirstOrDefaultAsync(ct);

        if ({entity} is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync({entity}, ct);
    }
}
```

`Where` then `Select` then `FirstOrDefaultAsync` — the projection is still what runs, so a missing row costs one narrow query.

The `return` after `Send.NotFoundAsync` matters: `Send` writes the response without unwinding the handler, so execution continues and the following `Send.OkAsync` throws.

### `{UseCase}Request.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public record {UseCase}Request(Guid {Entity}Id);
```

The route token `{{entity}Id}` binds by name to the `{Entity}Id` property.

### `{UseCase}RequestValidator.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}RequestValidator : Validator<{UseCase}Request>
{
    public {UseCase}RequestValidator()
    {
        RuleFor(v => v.{Entity}Id)
            .NotEmpty();
    }
}
```

`NotEmpty()` on a `Guid` rejects `Guid.Empty`, so an all-zeros ID gets a 400 rather than a pointless database round trip ending in 404.

### `{UseCase}Response.cs` and `{UseCase}Summary.cs`

Same shapes as above, minus the outer list.

---

## When a query should use a spec instead

Projection is the default, but reach for `.WithSpecification({Entity}Spec.{Query}(...))` when the filtering logic is shared with other call sites or is intricate enough to be worth naming and testing once. Add a static factory method to the existing spec class rather than a new class — one spec per aggregate keeps every query for it in one place.

Either way, a *query* still shouldn't materialise a tracked aggregate it only reads from.
