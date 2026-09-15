---
name: add-slice
description: Scaffold a slice in this Vertical Slice Architecture template — one use case in its own folder, with its HotChocolate resolver, input record and validator, plus the feature-level object type if it doesn't exist yet, plus tests. Use when the user says "add a slice", "add a use case", "add a query", "add a mutation", "add a field", "add a command", "expose X over the API", or names a use case such as "let users archive a team". Run `/add-entity` first if the use case needs a domain type that doesn't exist yet.
---

# Add a Slice

A **slice** is one use case, owning everything it needs from the GraphQL field to persistence, in its own folder. `CreateHero` is a slice. A **Feature** is the group of slices over the same aggregate, plus the object type that puts that aggregate in the schema — `Heroes` is a Feature, and it is not itself a slice. [`CONTEXT.md`](../../../CONTEXT.md) at the repo root defines both, along with Resolver, Object Type and the rest of the vocabulary. Use those words, and avoid the ones it lists under *Avoid*.

This skill adds a slice. It adds the feature-level files too, but only when the slice is the first one in that Feature.

Not every slice answers a request. `PowerLevelUpdated` is triggered by a domain event — the same folder-per-use-case shape, with a handler in place of a resolver.

## Read the live reference first

- `src/WebApi/Features/Heroes/CreateHero/` — the canonical mutation, three files
- `src/WebApi/Features/Heroes/GetAllHeroes/` — a paged, filtered, sorted list query
- `src/WebApi/Features/Teams/GetTeam/` — a single-item query
- `src/WebApi/Features/Teams/ExecuteMission/` — a mutation that turns an `ErrorOr` result into a typed error
- `src/WebApi/Features/Heroes/PowerLevelUpdated/` — a subscription and a domain event handler
- `src/WebApi/Features/Heroes/HeroType.cs` — the feature-level object type, filter and sort allow-lists, DataLoaders

The templates in `references/` follow these. If they disagree, the repo wins — follow it and fix the template.

## What you need to know before scaffolding

| Input | Notes |
|---|---|
| Feature | The plural noun that owns the aggregate (`Heroes`, `Teams`). Does one already exist, or is this its first slice? |
| Use case | Verb + noun, PascalCase (`CreateHero`, `ArchiveTeam`). This names the slice folder and the resolver class. |
| Query, mutation or subscription | A query reads, a mutation writes, a subscription pushes. |
| Field name | Derived from the method name with `Get` and `Async` removed, so `GetHeroes` is `heroes` and `ArchiveTeamAsync` is `archiveTeam`. Confirm it reads well in a document. |
| Input shape | Mutations take one input record. Queries take plain arguments. |
| Return type | An entity, so the client can ask for whatever it needs of it. Not a per-slice DTO. |
| Failure modes | Not found, conflict, validation — each is an `[Error<T>]` on the resolver. |

Ask about anything not stated. Guessing a field name produces a slice that compiles and is wrong.

## Steps

1. **Feature scaffolding** — only when this is the Feature's first slice: create `src/WebApi/Features/{Feature}/` and add `{Aggregate}Type.cs`, and for a list query also `{Aggregate}FilterType.cs` and `{Aggregate}SortType.cs`. Skip `{Feature}Feature.cs` unless the Feature registers its own services; most don't, and an empty one is noise. Template: [references/object-type.md](references/object-type.md).
2. **Slice folder** — `src/WebApi/Features/{Feature}/{UseCase}/`, namespace mirroring the folder. The resolver has to sit exactly two segments below `Features` — one for the Feature, one for the use case — or the architecture tests fail.
3. **The resolver** — `{UseCase}Query.cs`, `{UseCase}Mutation.cs` or `{UseCase}Subscription.cs`, a `static partial class` with the matching attribute. A mutation also gets `{UseCase}Input.cs` and `{UseCase}InputValidator.cs`, one type per file.
   - Mutation: [references/mutation-slice.md](references/mutation-slice.md)
   - Query: [references/query-slice.md](references/query-slice.md)
4. **Event-triggered slice** — when the use case reacts to a domain event rather than a request, the slice holds an `IDomainEventHandler<TEvent>` instead of a resolver: `src/WebApi/Features/{Feature}/{Event}/{Event}EventHandler.cs`. It belongs to the Feature that *consumes* the event, not the one that raises it. Handlers are found by an assembly scan, so there is nothing to register.
5. **Regenerate the schema** — `dotnet run --project src/WebApi -- schema export --output src/WebApi/schema.graphql`. Read the diff: it is the clearest statement of what you just published.
6. **Tests** — an integration test per slice, plus unit tests for any domain behaviour the slice added. Template: [references/tests.md](references/tests.md).

## What CI enforces

`tests/WebApi.ArchitectureTests/FeatureTests.cs` turns six of these into build failures rather than review comments:

- **Resolvers are named `*Query`, `*Mutation` or `*Subscription` and live in a slice namespace** — exactly two segments below `Features`. A slice folder nested deeper or shallower fails.
- **Resolvers are static classes.** The source generator emits the other half of a static partial; a non-static or non-partial class produces no schema field at all, and nothing fails to compile.
- **Every mutation input has an `AbstractValidator<TInput>` in the same slice.** GraphQL validates the shape of an input, never its content — it will accept an empty name and a power level of 500.
- **No slice depends on another slice.** This is the rule that makes it Vertical Slice Architecture rather than layers in disguise.
- **Resolvers take `ApplicationDbContext`** — not the `DbContext` base type, and not a second `DbContext`. The check reads IL, so a context pulled from the service provider is caught too.
- **An `[ObjectType<T>]` sits at feature level, never in a slice.** One Hero type serves every hero slice; declaring it inside one slice would make every other slice depend on that slice.

Run them with `dotnet test tests/WebApi.ArchitectureTests`. A failure here means the code broke a rule; fix the code, not the test.

`Schema/SchemaSnapshotTests.cs` fails separately until you regenerate `schema.graphql`.

## The checklist that catches the silent failures

These all compile, and only some of them have a test.

- **`static partial class` plus the operation attribute.** Miss either and the field never reaches the schema, silently.
- **Validate first.** `await validator.ValidateAndThrowAsync(input, cancellationToken)` is the first line of every mutation, and `[Error<InputValidationError>]` is on the resolver. Without the attribute the exception surfaces as an unhandled top-level error instead of a typed payload member.
- **`ThrowOnError()` on an `ErrorOr` result.** Ignoring the result saves a broken aggregate and answers 200.
- **An `[Error<T>]` for every failure the resolver can produce.** An undeclared exception leaves the typed union and becomes a generic error with no code.
- **No `OrderBy` in a list resolver.** The sorting middleware steps aside when the query is already ordered, so the `order` argument silently stops working.
- **Attribute order on a list query** — `[UsePaging]`, then `[UseFiltering<T>]`, then `[UseSorting<T>]`.
- **Business rules in the aggregate, not the resolver.** If the resolver contains an `if` about domain state, that check belongs on the entity, returning `ErrorOr<Success>`.
- **Load the child collections before mutating.** Only `HasMany` navigations need this — owned collections (`OwnsMany(...).ToJson()`, like `Hero.Powers`) always come with their parent. For a `HasMany`, call a spec factory that declares the `Include`s (`TeamSpec.ById` does; `HeroSpec.ById` declares none). With neither, the children arrive silently empty.
- **A new field on an object type needs a DataLoader**, not a navigation property. Nothing eager-loads navigations, so the property returns its empty backing list.

## Verification

```bash
dotnet build && dotnet build -c Release
dotnet test tests/WebApi.UnitTests tests/WebApi.ArchitectureTests
dotnet test tests/WebApi.IntegrationTests    # needs Docker or Podman running
```

Then exercise it for real — a green test suite doesn't prove the field is named what you think. Boot with `aspire start --isolated` (see the `aspire` skill), wait for the WebApi resource to go healthy, and run the operation in Nitro at `https://localhost:7255/graphql`. Confirm the success path *and* at least one failure path. Remember that GraphQL answers a failed request with HTTP 200 and an `errors` array, so read the body rather than the status code.

Full detail: [`.claude/rules/verification.md`](../../rules/verification.md).

## Guardrails

- **Don't add a `{Feature}Feature.cs` with an empty `ConfigureServices`.** Add it when the Feature actually has services to register.
- **Don't return a per-slice DTO.** Return the entity and let the client choose its fields; that is what the object type is for. A DTO per slice would put two shapes of Hero in one schema.
- **Don't share an input type between slices.** Duplication between slices is the design, not a smell — it's what lets one slice change without breaking another, and `Slices_Should_NotDependOnOtherSlices` enforces it. Give the second one a distinct name (`CreateHeroPowerInput` and `UpdateHeroPowerInput`), because two CLR types cannot share one GraphQL type name.
- **Don't put a GraphQL attribute on a domain type.** The schema binding lives in `Features/`, so the Domain project stays free of HotChocolate.
- **Don't touch another Feature's folder.** If the use case needs data from another aggregate, load it through that aggregate's spec — that's what `AddHeroToTeam` does.

## Keeping this skill honest

The `references/` templates are copies of the repo's shapes and will drift. When you find one that no longer matches the Heroes or Teams slices, update it as part of the same change.
