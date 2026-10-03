# Feature-level types template

These sit directly in `src/WebApi/Features/{Feature}/`, not in a slice: every slice in the feature
returns the same object type, and an architecture test fails a type declared inside a slice.

Reference: `src/WebApi/Features/Heroes/HeroType.cs` and `src/WebApi/Features/Teams/TeamType.cs`.

---

## Object type

`{Aggregate}Type.cs`

```csharp
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature};

[ObjectType<{Aggregate}>]
public static partial class {Aggregate}Type
{
    static partial void Configure(IObjectTypeDescriptor<{Aggregate}> descriptor)
    {
        descriptor.Description("One sentence about what this type is.");

        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Name);
        descriptor.Field(x => x.CreatedAt);
        descriptor.Field(x => x.UpdatedAt);
    }

    /// <summary>A related collection, batched.</summary>
    public static async Task<IReadOnlyList<{Child}>> Get{Children}Async(
        [Parent] {Aggregate} {aggregate},
        I{Children}By{Aggregate}IdDataLoader {children}By{Aggregate}Id,
        CancellationToken cancellationToken)
        => await {children}By{Aggregate}Id.LoadAsync({aggregate}.Id, cancellationToken) ?? [];
}
```

Two rules, both learned the hard way.

- **`BindFieldsExplicitly`, then one `Field` call per exposed member.** The default binds every
  public member of the entity, *including its methods*: `Team.ExecuteMission` becomes a schema
  field, and its `ErrorOr<Success>` return value drags the ErrorOr types into the public schema,
  where `Error` collides with the error interface the mutation conventions own. The schema then
  fails to build, and the message names neither the entity nor the method.
- **Resolve navigation properties with a DataLoader.** Nothing eager-loads `Team.Heroes`, so the
  property returns its empty backing list and the client reads that as "no heroes". A resolver
  method named `Get{Children}Async` replaces the property's field.

The binding lives here rather than as attributes on the entity, so the Domain project stays free of
HotChocolate — an architecture test enforces that.

---

## DataLoaders

`{Aggregate}DataLoaders.cs`

```csharp
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.Features.{Feature};

public static class {Aggregate}DataLoaders
{
    /// <summary>One {aggregate} per key.</summary>
    [DataLoader]
    public static async Task<Dictionary<{Aggregate}Id, {Aggregate}>> Get{Aggregate}ByIdAsync(
        IReadOnlyList<{Aggregate}Id> ids,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.{Aggregates}
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

    /// <summary>Many children per key.</summary>
    [DataLoader]
    public static async Task<Dictionary<{Aggregate}Id, {Child}[]>> Get{Children}By{Aggregate}IdAsync(
        IReadOnlyList<{Aggregate}Id> ids,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
        => await dbContext.{Children}
            .Where(c => ids.Contains(c.{Aggregate}Id))
            .GroupBy(c => c.{Aggregate}Id)
            .Select(g => new { g.Key, Items = g.ToArray() })
            .ToDictionaryAsync(g => g.Key, g => g.Items, cancellationToken);
}
```

- The generator names the interface after the method, with `Get` and `Async` removed:
  `GetHeroesByTeamIdAsync` becomes `IHeroesByTeamIdDataLoader`. Inject that, never the class.
- The generated loader returns null for a key with no rows, so callers coalesce to `[]`.
- Each DataLoader runs in its own scope, and therefore its own `DbContext`.
- When the foreign key is a shadow property, query through the parent instead:
  `dbContext.Teams.Where(...).Select(t => new { t.Id, Missions = t.Missions.ToArray() })`.
