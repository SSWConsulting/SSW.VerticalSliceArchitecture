# Query slice templates

Placeholders as in [command-slice.md](command-slice.md).

A query reads and projects. It never mutates, never calls `SaveChangesAsync`, and never loads a full aggregate when a projection will do — projecting in the `Select` keeps the SQL narrow and skips change tracking entirely.

---

## List query — paged and sorted

Five files: endpoint, request, validator, response, summary. Every list endpoint pages — returning a whole table is a bug waiting for the table to grow — so there is always something to bind and validate. This is `GetAllHeroes`. The conventions are in [architecture.md](../../../rules/architecture.md) § Paging & Sorting.

### `{UseCase}Endpoint.cs`

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}Endpoint(ApplicationDbContext dbContext)
    : Endpoint<{UseCase}Request, PagedList<{UseCase}Response>>
{
    public override void Configure()
    {
        Get("/");
        Group<{Feature}Group>();
        Description(x => x.WithName("{UseCase}"));
    }

    public override async Task HandleAsync({UseCase}Request req, CancellationToken ct)
    {
        var paging = PagingParams.From(req.Page, req.PageSize);
        var spec = {Entity}Spec.Paged(paging, req.SortBy, SortDirections.From(req.SortDirection));

        var {entities} = await dbContext.{Entities}.ToPagedListAsync(
            spec,
            x => new {UseCase}Response(
                x.Id.Value,
                x.Name,
                x.{Children}.Select(c => new {UseCase}Response.{Child}Dto(c.Name, c.Value)).ToList()),
            ct);

        await Send.OkAsync({entities}, ct);
    }
}
```

Project straight into the response DTO inside the projection. That means no `Include` and no tracked entities — EF translates the whole thing to one query returning only the columns the response needs. Loading entities and mapping afterwards fetches columns nobody reads and pays for change tracking the query will never use.

`ToPagedListAsync` runs the page and its total count off the same spec, and reads the envelope's `page`/`pageSize` back off that spec — pass one that didn't come from a `Paged` factory and it throws rather than describing a window it never fetched.

`x.Id.Value` unwraps the Vogen ID. The API contract speaks `Guid`; strongly typed IDs stay inside the domain.

The `Paged` factory and its sort allow-list live on the aggregate's spec, not here — see [domain.md](../../../rules/domain.md) § Sorting & paging.

### `{UseCase}Request.cs`

```csharp
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public record {UseCase}Request : PagedRequest;
```

Inheriting `PagedRequest` is what keeps `page` / `pageSize` / `sortBy` / `sortDirection` spelled the same on every list endpoint. Add slice-specific filter properties to this record; don't redeclare the paging ones.

### `{UseCase}RequestValidator.cs`

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}RequestValidator : PagedRequestValidator<{UseCase}Request, {Entity}>
{
    public {UseCase}RequestValidator()
        : base({Entity}Spec.SortColumns)
    {
    }
}
```

It must be a `PagedRequestValidator`, not a bare `Validator<{UseCase}Request>` — the sort allow-list rules live in that base class, and an architecture test enforces it. Without them an unknown sort column reaches the primitives, which throw, turning the documented 400 into a 500. Add slice-specific rules in the constructor body; the inherited ones still run.

No page or page-size rules: `PagingParams.From` clamps those, so there is nothing left to reject.

### `{UseCase}Response.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public record {UseCase}Response(
    Guid Id,
    string Name,
    IReadOnlyList<{UseCase}Response.{Child}Dto> {Children})
{
    public record {Child}Dto(string Name, int Value);
}
```

For a list use case the response record is **one item**, not the list — the endpoint returns `PagedList<{UseCase}Response>`, so the envelope is the same shape on every list endpoint and a generated client can treat paging uniformly.

### `{UseCase}Summary.cs`

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};
using SSW.VerticalSliceArchitecture.Common.Pagination;

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}Summary : Summary<{UseCase}Endpoint>
{
    public {UseCase}Summary()
    {
        Summary = "Get a page of {entities}";
        Description = "Retrieves a page of {entities}, wrapped in the standard paged envelope " +
                      "(items plus page, pageSize, totalCount, totalPages).";

        Params[nameof({UseCase}Request.Page)] =
            $"1-based page number. Defaults to {PagingParams.FirstPage}; anything lower is treated as the first page.";
        Params[nameof({UseCase}Request.PageSize)] =
            $"Items per page. Defaults to {PagingParams.DefaultPageSize} and is clamped to at most {PagingParams.MaxPageSize}.";
        Params[nameof({UseCase}Request.SortBy)] =
            $"Column to sort by — one of: {string.Join(", ", {Entity}Spec.SortColumns.AllowedColumns)}. Anything else returns 400.";
        Params[nameof({UseCase}Request.SortDirection)] =
            $"Sort direction — one of: {string.Join(", ", SortDirections.Allowed)}. Defaults to ascending.";

        // Response example
        Response(200, "{Entities} retrieved successfully",
            example: new PagedList<{UseCase}Response>(
                Items:
                [
                    new {UseCase}Response(
                        Id: Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                        Name: "Peter Parker",
                        {Children}:
                        [
                            new {UseCase}Response.{Child}Dto("Web Slinging", 3)
                        ])
                ],
                Page: 1,
                PageSize: 10,
                TotalCount: 1));

        Response(400, "Unknown sort column or sort direction");
    }
}
```

`Response(200, ..., example: ...)` documents the output shape, where a command's summary uses `ExampleRequest` for the input. The `Params` entries are what put the four query parameters in Swagger with their defaults and allowed values — build them from the constants and the allow-list so the docs can't drift from the code.

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
