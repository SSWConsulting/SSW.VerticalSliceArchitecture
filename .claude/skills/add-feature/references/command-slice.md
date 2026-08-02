# Command slice templates

Placeholders: `{Feature}` the plural feature (`Heroes`), `{UseCase}` the verb+noun (`CreateHero`), `{Entity}` the aggregate (`Hero`).

The namespace root (`SSW.VerticalSliceArchitecture`) is rewritten by `dotnet new` on instantiation, so use it verbatim.

`FastEndpoints`, `FluentValidation`, `ErrorOr`, `Microsoft.EntityFrameworkCore`, `Ardalis.Specification` and `Common.Persistence` are global usings — the templates below only add what's missing (the domain namespace, and `Ardalis.Specification.EntityFrameworkCore` for `.WithSpecification`).

---

## Feature group

`src/WebApi/Features/{Feature}/{Feature}Group.cs`

One per feature. The prefix becomes the route segment, the Swagger tag, and the group name at once.

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature};

public class {Feature}Group : Group
{
    public {Feature}Group()
    {
        // NOTE: The prefix is used as the tag and group name
        base.Configure("{prefix}", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}
```

`{prefix}` is lowercase plural (`heroes`, `teams`). Routes resolve to `/api/{prefix}` + whatever the endpoint declares.

---

## Feature DI registration (usually skip this)

`src/WebApi/Features/{Feature}/{Feature}Feature.cs`

Only add this when the feature has its own services to register. `FeatureDiscovery` reflects over `IFeature` implementations at startup and invokes `ConfigureServices`, so an empty one costs a reflection hit and tells the next reader a lie about the feature having dependencies.

```csharp
using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace SSW.VerticalSliceArchitecture.Features.{Feature};

public sealed class {Feature}Feature : IFeature
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<I{Something}, {Something}>();
    }
}
```

---

## Create command — body only, returns the new ID

Five files. This is `CreateHero`.

### `{UseCase}Endpoint.cs`

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}Endpoint(ApplicationDbContext dbContext)
    : Endpoint<{UseCase}Request, {UseCase}Response>
{
    public override void Configure()
    {
        Post("/");
        Group<{Feature}Group>();
        Description(x => x.WithName("{UseCase}"));
    }

    public override async Task HandleAsync({UseCase}Request req, CancellationToken ct)
    {
        var {entity} = {Entity}.Create(req.Name);

        dbContext.{Entities}.Add({entity});
        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new {UseCase}Response({entity}.Id.Value), ct);
    }
}
```

The `DbContext` arrives through the primary constructor — no `[Inject]`, no property injection. `{entity}.Id.Value` unwraps the Vogen ID to the `Guid` the response exposes; strongly typed IDs stay inside the domain and don't leak into the API contract.

### `{UseCase}Request.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public record {UseCase}Request(
    string Name,
    IEnumerable<{UseCase}Request.{Child}Dto> {Children})
{
    public record {Child}Dto(string Name, int Value);
}
```

Nested DTOs, not shared ones. A request record belongs to exactly one slice — that's what lets the slice's contract change without a ripple.

### `{UseCase}Response.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public record {UseCase}Response(Guid Id);
```

### `{UseCase}RequestValidator.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}RequestValidator : Validator<{UseCase}Request>
{
    public {UseCase}RequestValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength({Entity}.NameMaxLength);

        RuleForEach(v => v.{Children})
            .ChildRules(child =>
            {
                child.RuleFor(c => c.Value)
                    .InclusiveBetween(1, 10);
            });
    }
}
```

`Validator<T>` is the FastEndpoints wrapper around FluentValidation — it's discovered and run before `HandleAsync`, and a failure auto-returns 400. Nothing registers it by hand.

Read max lengths off the entity's `const` so the API rejects an over-long value with a 400 instead of letting the domain guard throw a 500 deeper in.

### `{UseCase}Summary.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}Summary : Summary<{UseCase}Endpoint>
{
    public {UseCase}Summary()
    {
        Summary = "Create a new {entity}";
        Description = "Creates a new {entity} with the specified name. Returns the ID of the created {entity}.";

        // Request example
        ExampleRequest = new {UseCase}Request(
            Name: "Peter Parker",
            {Children}:
            [
                new {UseCase}Request.{Child}Dto("Web Slinging", 1)
            ]);

        // Also, add response examples if needed
    }
}
```

The summary is what makes Swagger readable. Write a real example — placeholder strings make the docs worse than no example.

---

## Update command — route parameter, returns 204

This is `UpdateHero`. `Endpoint<TRequest>` with no response type is the 204 shape.

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

public class {UseCase}Endpoint(ApplicationDbContext dbContext)
    : Endpoint<{UseCase}Request>
{
    public override void Configure()
    {
        Put("/{{entity}Id}");
        Group<{Feature}Group>();
        Description(x => x
            .WithName("{UseCase}")
            .Produces(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync({UseCase}Request req, CancellationToken ct)
    {
        var {entity}Id = {Entity}Id.From(req.{Entity}Id);

        var {entity} = await dbContext.{Entities}
            .Include(x => x.{Children})
            .FirstOrDefaultAsync(x => x.Id == {entity}Id, ct);

        if ({entity} is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        {entity}.Rename(req.Name);

        await dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
```

```csharp
public record {UseCase}Request(Guid {Entity}Id, string Name);
```

Whether that `Include` is load-bearing depends on how the children are mapped, so check the EF configuration before you keep or drop it:

- **Owned collections** — `OwnsMany(...).ToJson()`, as `Hero.Powers` is. EF loads these with their parent automatically, so the `Include` is belt-and-braces. It's why `HeroSpec.ById` gets away with declaring none, and why `CreateHeroCommandTests` can assert on `Powers` after a bare `Set<Hero>()` query.
- **Referenced navigations** — `HasMany`, as `Team.Missions` and `Team.Heroes` are. These do *not* come for free. Without an `Include` here or a spec that declares one, the collection arrives empty with no error, and mutating from there quietly writes wrong data.

Keep the line for `HasMany` children; drop it for owned ones, or when the aggregate has no children at all.

Route parameter and body bind into the same record — the route token `{{entity}Id}` matches the `{Entity}Id` property by name, so there's no separate binding attribute.

The `return` after `Send.NotFoundAsync` is load-bearing. `Send` writes the response but doesn't unwind the handler, so without it execution continues and the second `Send` throws.

`Produces(StatusCodes.Status404NotFound)` has to be declared for every non-200 status the handler can send, or it's absent from the OpenAPI document and generated clients won't model it.

---

## Command that loads through specs

When the use case mutates an aggregate with child collections, load it through its spec. Needs `using Ardalis.Specification.EntityFrameworkCore;`. This is `AddHeroToTeam`.

```csharp
using Ardalis.Specification.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

// ...

var {entity} = dbContext.{Entities}
    .WithSpecification({Entity}Spec.ById({entity}Id))
    .FirstOrDefault();

if ({entity} is null)
{
    await Send.NotFoundAsync(ct);
    return;
}
```

The `Include`s live in the spec — `TeamSpec.ById` pulls `Missions` and `Heroes`, so every caller loading a team to mutate it gets the same complete aggregate. That's why the spec is the default for a load-then-mutate path.

A spec only brings what it declares, though, so reaching for one isn't automatically enough — check that the factory method you're calling loads the collections you're about to touch. `HeroSpec.ById` is a bare `Where` with no `Include` at all, which is fine only because `Hero.Powers` is owned and comes with its parent regardless. Point that same shape at a `HasMany` navigation and the collection arrives empty with no error, and mutating from there quietly writes wrong data.

---

## Command whose domain call can fail

When the aggregate returns `ErrorOr<Success>`, translate the error rather than throwing. This is `ExecuteMission`.

```csharp
var result = {entity}.{DoSomething}(req.Description);

if (result.IsError)
{
    result.Errors.ForEach(e => AddError(e.Description, e.Code));
    await Send.ErrorsAsync(cancellation: ct);
    return;
}

await dbContext.SaveChangesAsync(ct);
await Send.NoContentAsync(ct);
```

Every domain error is carried across with both its description and its code, so the `{Entity}.{Condition}` codes defined in `{Entity}Errors` reach the client instead of being flattened into prose. `Send.ErrorsAsync` renders them as the standard problem-details 400.

`ThrowError("...")` exists for the ad-hoc case where the handler itself spots a problem the validator couldn't express. When the failure came from the domain, use the shape above — it preserves the codes.

The rule the endpoint must not break: the decision about *whether* the operation is allowed lives on the aggregate. The endpoint asks and reports; it doesn't decide.
