---
paths:
  - "src/WebApi/Features/**/*"
  - "src/WebApi/Common/GraphQL/**/*"
  - "src/WebApi/Common/Interfaces/**/*"
  - "src/WebApi/Host/**/*"
---

# Architecture — Vertical Slices + HotChocolate GraphQL

## Project layout

```
src/Domain/                 # the model, with no framework dependencies
src/WebApi/                 # the schema, the slices, and persistence
src/ServiceDefaults/        # Aspire telemetry, health checks, service discovery
```

An architecture test pins the Domain project's package list to Ardalis.Specification, ErrorOr and
Vogen. Adding EF Core, ASP.NET Core or HotChocolate to it fails the build, which is the point of the
split: the model must not learn how it is stored or served.

## Slice layout

```
src/WebApi/Features/{Feature}/
  {Feature}Feature.cs       # IFeature.ConfigureServices — only if the feature needs DI
  {Aggregate}Type.cs        # [ObjectType<T>] — how the aggregate appears in the schema
  {Aggregate}FilterType.cs  # the fields callers may filter by
  {Aggregate}SortType.cs    # the fields callers may sort by
  {Aggregate}DataLoaders.cs # batched loads for this feature's related data
  {UseCase}/                # one folder per use case
    {UseCase}Query.cs       # or {UseCase}Mutation.cs / {UseCase}Subscription.cs
    {UseCase}Input.cs       # mutations only
    {UseCase}InputValidator.cs
```

Reference slices: `Features/Heroes/CreateHero/` (mutation), `Features/Heroes/GetAllHeroes/` (list
query), `Features/Teams/GetTeam/` (single item), `Features/Heroes/PowerLevelUpdated/` (subscription
plus a domain event handler). Copy their shape rather than reinventing.

## Conventions

- Namespace mirrors the folder: `Features.Heroes.CreateHero`.
- A resolver class is `static partial` and carries `[QueryType]`, `[MutationType]` or
  `[SubscriptionType]`. The source generator emits the other half; a class that is neither static
  nor partial produces no field, and nothing fails to compile.
- The field name comes from the method name, with `Get` and `Async` removed: `GetHeroes` becomes
  `heroes`, `CreateHeroAsync` becomes `createHero`.
- One resolver method per slice. A second field in the same class is a second use case.
- Services in the signature — `ApplicationDbContext`, `IValidator<T>`, a DataLoader — stay out of
  the schema. Only the input parameter becomes an argument.
- `[assembly: Module("WebApiTypes")]` in `AssemblyInfo.cs` is what makes the generator run. Without
  it the schema comes up empty.

## Object types

An aggregate reaches the schema through `[ObjectType<T>]` at feature level, never through attributes
on the entity. Two rules matter.

- **Bind fields explicitly.** Call `descriptor.BindFieldsExplicitly()`, then one `Field` call per
  exposed member. The default binds every public member, including methods: `Team.ExecuteMission`
  would become a schema field, and its `ErrorOr` return value would drag the result types into the
  public schema.
- **Resolve navigation properties with a DataLoader.** Nothing eager-loads `Team.Heroes`, so the
  property returns its empty backing list and the client reads that as "no heroes". Declare a
  resolver method on the object type that loads through a DataLoader instead.

## Paging, filtering and sorting

Every list query is a Relay connection. The primitives are HotChocolate's; the allow-lists are ours.

- **Attributes** — `[UsePaging]`, `[UseFiltering<TFilter>]`, `[UseSorting<TSort>]`, in that order.
  Paging runs last, so filtering and sorting shape the query before it is sliced.
- **Defaults and limits** — 10 per page, 50 maximum, `totalCount` included. Set once in
  `GraphQlExt.ModifyPagingOptions`. Asking for more than the maximum is an error, not a clamp.
- **Allow-lists** — a `FilterInputType<T>` and a `SortInputType<T>` per aggregate, both using
  `BindFieldsExplicitly`. The default binds every property, which publishes the audit columns and
  lets a client write a query no index supports.
- **Return the query unordered.** The sorting middleware steps aside when the resolver already
  ordered the queryable, so an `OrderBy` in the resolver silently disables the `order` argument.

## Errors

A business failure is a typed member of the mutation's payload, not a transport failure. A mutation
that can fail declares `[Error<TError>]`, and HotChocolate adds that type to the payload's error
union.

- The domain returns `ErrorOr`. The slice turns it into an exception at the edge with
  `result.ThrowOnError()`, which maps `ErrorType.NotFound` to `NotFoundError` and
  `ErrorType.Conflict` to `ConflictError`. An unmapped error type throws instead of falling back, so
  a missing mapping is loud.
- Input validation is FluentValidation, called explicitly:
  `await validator.ValidateAndThrowAsync(input, cancellationToken)` as the first line of the
  mutation. GraphQL checks the shape of an input, never its content.
- A failure inside a domain event handler throws `EventualConsistencyException`.
- Anything else surfaces as a top-level GraphQL error.

## Domain events

`IDomainEvent` is a marker in the Domain project, with no library behind it. An aggregate raises one
with `AddDomainEvent`, `DispatchDomainEventsInterceptor` collects them after `SaveChanges`, and
`EventualConsistencyMiddleware` publishes them once the response is written.

A handler implements `IDomainEventHandler<TEvent>` and lives in the slice that *reacts* to the
event, not the one that raises it. Handlers are found by an assembly scan, so one event can have
several: `PowerLevelUpdatedEvent` has a Teams handler that recalculates the team total, and a Heroes
handler that pushes the subscription message. Each handler runs in its own dependency injection
scope, and therefore its own `DbContext`.

## Gotchas

- A resolver class that is not `static partial` contributes nothing, silently.
- A new strongly typed ID needs a scalar and a `BindRuntimeType` call in `GraphQlExt`, or the schema
  fails to build on a duplicate type name. See [database.md](database.md).
- An `OrderBy` in a list resolver disables the `order` argument.
- Loading aggregates without an Ardalis spec returns silently incomplete data. See [domain.md](domain.md).
- Changing a resolver signature changes the schema, and the snapshot test fails until you
  regenerate it: `dotnet run --project src/WebApi -- schema export --output src/WebApi/schema.graphql`.
