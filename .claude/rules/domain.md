---
paths:
  - "src/Domain/**/*"
---

# Domain

The domain is its own project, `src/Domain`, and depends on three packages only:
Ardalis.Specification, ErrorOr and Vogen. `DomainTests.Domain_Should_OnlyReferenceAllowedPackages`
fails the build on anything else, so EF Core, ASP.NET Core and HotChocolate stay out. Persistence
and the schema reference the domain; the domain references neither.

## Entities & Aggregates

- Inherit `Entity<TId>` for plain entities, `AggregateRoot<TId>` when the type raises domain events.
- Static `Create(...)` factory + `private` parameterless ctor for EF Core. Reference: `Hero.cs`.
- Length caps live on the entity as `public const int {Property}MaxLength`, referenced by both the property setter guard and the EF configuration.
- Guards belong in the property setter (using `field`), not in the factory. The setter is the only spot every assignment path goes through.

## Strongly Typed IDs

- `[ValueObject<Guid>]` from Vogen. IDs use `Guid.CreateVersion7()` for time-ordered values.
- **Every new ID must also be registered twice outside the domain**, and both are startup failures
  when missed:
  - `src/WebApi/Common/Persistence/VogenEfCoreConverters.cs` — add `[EfCoreConverter<YourId>]`.
  - `src/WebApi/Common/GraphQL/Scalars/` — add a scalar deriving from `VogenGuidIdType<TId>`, and a
    `BindRuntimeType` call in `GraphQlExt`. Without it HotChocolate infers an object type from the
    struct and the schema fails to build on the duplicate name.

## Specifications

- Use Ardalis.Specification for any non-trivial query and for loading aggregates.
- One spec class per aggregate: `src/Domain/{Aggregate}/{Aggregate}Spec.cs`, extending `Specification<T>`. Add a static factory method per query so all of an aggregate's queries live in one discoverable place.
- Apply via `.WithSpecification(HeroSpec.ById(id))` on the DbSet.
- The base is `Specification<T>`, not `SingleResultSpecification<T>`: the same class holds single-result and list queries, and the single-result marker only matters to the Ardalis repository, which this template doesn't use.

```csharp
public sealed class HeroSpec : Specification<Hero>
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

### Sorting & paging

Specs do not page or sort. Both are GraphQL middleware, and the fields a caller may filter and sort
by are allow-listed per aggregate in the API project — see [architecture.md](architecture.md).

## Value Objects

`record` types for structural equality. Encapsulate invariants in the constructor.

## Domain Events

- Declare as a `record` implementing `IDomainEvent` — a marker in this project, so that raising an
  event costs the domain no dependency. Raise via `AddDomainEvent(...)` on the aggregate. Reference:
  `PowerLevelUpdatedEvent.cs`.
- `DispatchDomainEventsInterceptor` collects them after `SaveChangesAsync()`, and
  `DomainEventDispatcher` runs every `IDomainEventHandler<TEvent>` found in the API assembly. Each
  handler gets its own scope, and therefore its own `DbContext`.
- If a handler cannot complete, throw `EventualConsistencyException`.
- Example chain: a hero's powers change, and `PowerLevelUpdatedEventHandler` in the Teams feature
  recalculates the team total while `PowerLevelUpdatedPublisher` in the Heroes feature pushes the
  subscription message.

## Domain Errors

`public static class {Entity}Errors` containing `Error` constants (using ErrorOr). Example: `HeroErrors.NotFound`.
