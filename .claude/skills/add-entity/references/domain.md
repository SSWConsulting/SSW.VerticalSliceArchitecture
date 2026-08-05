# Domain templates

Placeholders: `{Aggregate}` is the folder / aggregate name (`Heroes`), `{Entity}` the type (`Hero`), `{Entities}` its plural (`Heroes`).

The namespace root (`SSW.VerticalSliceArchitecture`) is rewritten by `dotnet new` when the template is instantiated, so keep using it verbatim — it will match whatever the generated project is called. If you're unsure of the current root, read `src/WebApi/GlobalUsings.cs`.

Types available without a `using` (from `src/WebApi/GlobalUsings.cs`): `Vogen`, `ErrorOr`, `Ardalis.Specification`, `FluentValidation`, `FastEndpoints`, `Microsoft.EntityFrameworkCore`, and the `ArgumentException` / `ArgumentNullException` / `ArgumentOutOfRangeException` static guard helpers (`ThrowIfNullOrWhiteSpace`, `ThrowIfGreaterThan`, `ThrowIfLessThan`, `ThrowIfNull`).

---

## Aggregate root

`src/WebApi/Common/Domain/{Aggregate}/{Entity}.cs`

An aggregate root is the transactional boundary: it's what gets a `DbSet`, what specs load, and the only thing that may raise domain events.

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.Base;

namespace SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

// Ensure stongly typed IDs are registered in 'VogenEfCoreConverters'
// For strongly typed IDs, check out the rule: https://www.ssw.com.au/rules/do-you-use-strongly-typed-ids/
[ValueObject<Guid>]
public readonly partial struct {Entity}Id;

public class {Entity} : AggregateRoot<{Entity}Id>
{
    public const int NameMaxLength = 100;

    private readonly List<{Child}> _{children} = [];

    public string Name
    {
        get;
        private set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Name));
            ThrowIfGreaterThan(value.Length, NameMaxLength, nameof(Name));
            field = value;
        }
    } = null!;

    public IReadOnlyList<{Child}> {Children} => _{children}.AsReadOnly();

    private {Entity}() { } // Needed for EF Core

    public static {Entity} Create(string name)
    {
        var {entity} = new {Entity} { Id = {Entity}Id.From(Guid.CreateVersion7()), Name = name };

        return {entity};
    }

    public void Rename(string name) => Name = name;
}
```

Four things carry weight here:

- **`private set` with the `field` keyword.** The guard sits in the setter, not in `Create`, because the setter is the only path every assignment goes through — construction, later mutation, and EF materialisation alike. A guard in the factory is bypassed the moment anything else sets the property.
- **`public const int {Property}MaxLength`.** The EF configuration reads this same constant, so the database column and the runtime guard can't drift apart.
- **`private {Entity}() { }`.** EF Core needs it, and `DomainTests.EntitiesAndAggregates_Should_HavePrivateParameterlessConstructor` fails the build if it's missing or public.
- **`Guid.CreateVersion7()`.** Time-ordered, so it doesn't fragment the clustered index the way a v4 GUID does.

Drop the `_{children}` collection entirely if the aggregate has no child entities.

### Behaviour that can fail

Return `ErrorOr<Success>` rather than throwing when a caller could reasonably hit the condition — an unavailable team isn't exceptional, it's an expected outcome the endpoint should translate to a response. Reserve the `ThrowIf*` guards for programmer error. `Team.ExecuteMission` is the reference:

```csharp
public ErrorOr<Success> ExecuteMission(string description)
{
    ThrowIfNullOrWhiteSpace(description);

    if (Status != {Entity}Status.Available)
        return {Entity}Errors.NotAvailable;

    // ... mutate state ...

    return new Success();
}
```

---

## Child entity

`src/WebApi/Common/Domain/{Aggregate}/{Child}.cs`

Same shape, but inherits `Entity<TId>` instead of `AggregateRoot<TId>`, gets no `DbSet`, and is created through its parent. Only the aggregate root raises domain events.

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.Base;

namespace SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

[ValueObject<Guid>]
public readonly partial struct {Child}Id;

public class {Child} : Entity<{Child}Id>
{
    public const int DescriptionMaxLength = 200;

    public string Description
    {
        get;
        private set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Description));
            ThrowIfGreaterThan(value.Length, DescriptionMaxLength, nameof(Description));
            field = value;
        }
    } = null!;

    private {Child}() { } // Needed for EF Core

    // internal so only the owning aggregate can create one
    internal static {Child} Create(string description) =>
        new() { Id = {Child}Id.From(Guid.CreateVersion7()), Description = description };
}
```

Its ID still needs registering in `VogenEfCoreConverters`.

---

## Value object

`src/WebApi/Common/Domain/{Aggregate}/{ValueObject}.cs`

No ID, no lifecycle, structural equality. Use one when the concept is defined entirely by its values — an amount, a coordinate, a named power. Reference: `Power.cs`.

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

public record {ValueObject} : IValueObject
{
    public const int NameMaxLength = 50;

    // Private setters needed for EF
    public string Name
    {
        get;
        private set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Name));
            ThrowIfGreaterThan(value.Length, NameMaxLength, nameof(Name));
            field = value;
        }
    } = null!;

    public {ValueObject}(string name)
    {
        Name = name;
    }
}
```

`record` rather than `class` — structural equality is the point. `IValueObject` is what the architecture tests match on, so it isn't decorative.

---

## Errors

`src/WebApi/Common/Domain/{Aggregate}/{Entity}Errors.cs`

```csharp
namespace SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

public static class {Entity}Errors
{
    public static readonly Error NotFound = Error.NotFound(
        "{Entity}.NotFound",
        "{Entity} is not found");

    public static readonly Error NotAvailable = Error.Conflict(
        "{Entity}.NotAvailable",
        "{Entity} is not available");
}
```

The code string is `{Entity}.{Condition}` — it's what surfaces in the problem-details payload, so it's an API contract, not a log message.

---

## Specification

`src/WebApi/Common/Domain/{Aggregate}/{Entity}Spec.cs`

One spec class per aggregate, one static factory method per query. That keeps every query for an aggregate in one discoverable place instead of scattered across slices.

```csharp
namespace SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

// For more on the Specification Pattern see: https://www.ssw.com.au/rules/use-specification-pattern/
public sealed class {Entity}Spec : SingleResultSpecification<{Entity}>
{
    public static {Entity}Spec ById({Entity}Id {entity}Id)
    {
        var spec = new {Entity}Spec();
        spec.Query.Where(x => x.Id == {entity}Id);
        return spec;
    }

    public static {Entity}Spec ByIdWith{Children}({Entity}Id {entity}Id)
    {
        var spec = new {Entity}Spec();
        spec.Query
            .Where(x => x.Id == {entity}Id)
            .Include(x => x.{Children});
        return spec;
    }
}
```

Use a spec whenever you load an aggregate you intend to mutate: the `Include`s live in the spec, so every caller gets the same complete aggregate instead of each remembering its own list of navigations.

A spec only brings what it declares, though. `ById` above is a bare `Where` — it's `ByIdWith{Children}` that loads the children.

Which of those you need depends on the EF mapping. Owned collections (`OwnsMany(...).ToJson()`) always come with their parent, which is why the repo's `HeroSpec.ById` declares no `Include` and still yields a Hero with its `Powers`. A `HasMany` navigation doesn't: load one with neither a spec that includes it nor an explicit `.Include(...)` and you get an empty collection and no error, with the bug showing up later as silently lost data.

Applied at the call site with `.WithSpecification(...)`, which needs `using Ardalis.Specification.EntityFrameworkCore;`:

```csharp
var {entity} = dbContext.{Entities}
    .WithSpecification({Entity}Spec.ById({entity}Id))
    .FirstOrDefault();
```

---

## Domain event

`src/WebApi/Common/Domain/{Aggregate}/{Event}Event.cs`

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.Base.EventualConsistency;

namespace SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

public record {Event}Event({Entity} {Entity}) : IEvent
{
    public static readonly Error {Dependency}NotFound = EventualConsistencyError.From(
        code: "{Event}.{Dependency}NotFound",
        description: "{Dependency} not found");
}
```

Raised from inside the aggregate:

```csharp
AddDomainEvent(new {Event}Event(this));
```

`DispatchDomainEventsInterceptor` dispatches these after `SaveChangesAsync()`, and handlers run inside the same transaction. So a handler that can't complete must throw `EventualConsistencyException` (built from the `EventualConsistencyError` above) rather than swallowing the failure — `EventualConsistencyMiddleware` turns it into the right HTTP response. Handlers live in the consuming slice, not the domain: `src/WebApi/Features/{Feature}/{Event}/{Event}EventHandler.cs`. Reference: `Features/Teams/PowerLevelUpdated/PowerLevelUpdatedEventHandler.cs`.
