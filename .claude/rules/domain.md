---
paths:
  - "src/WebApi/Common/Domain/**/*"
---

# Domain

## Entities & Aggregates

- Inherit `Entity<TId>` for plain entities, `AggregateRoot<TId>` when the type raises domain events.
- Static `Create(...)` factory + `private` parameterless ctor for EF Core. Reference: `Hero.cs`.
- Length caps live on the entity as `public const int {Property}MaxLength`, referenced by both the property setter guard and the EF configuration.
- Guards belong in the property setter (using `field`), not in the factory. The setter is the only spot every assignment path goes through.

## Strongly Typed IDs

- `[ValueObject<Guid>]` from Vogen. IDs use `Guid.CreateVersion7()` for time-ordered values.
- **Every new ID must also be registered in `src/WebApi/Common/Persistence/VogenEfCoreConverters.cs`** with `[EfCoreConverter<YourId>]`. The app fails at startup if one is missing.

## Specifications

- Use Ardalis.Specification for any non-trivial query and for loading aggregates.
- One spec class per aggregate: `Common/Domain/{Aggregate}/{Aggregate}Spec.cs`, extending `SingleResultSpecification<T>`. Add a static factory method per query so all of an aggregate's queries live in one discoverable place.
- Apply via `.WithSpecification(HeroSpec.ById(id))` on the DbSet.

```csharp
public sealed class HeroSpec : SingleResultSpecification<Hero>
{
    public static HeroSpec ById(HeroId heroId)
    {
        var spec = new HeroSpec();
        spec.Query.Where(h => h.Id == heroId);
        return spec;
    }

    // Add further factory methods here as new queries are needed
}
```

## Value Objects

`record` types for structural equality. Encapsulate invariants in the constructor.

## Domain Events

- Inherit from `DomainEvent`. Raise via `AddDomainEvent(...)` on the aggregate.
- Dispatched by `DispatchDomainEventsInterceptor` after `SaveChangesAsync()`. Handlers run in the same transaction.
- If a handler can't complete, throw `EventualConsistencyException` so the middleware translates it for HTTP.
- Example chain: `PowerLevelUpdatedEventHandler` recalculates team power when a hero's powers change.

## Domain Errors

`public static class {Entity}Errors` containing `Error` constants (using ErrorOr). Example: `HeroErrors.NotFound`.
