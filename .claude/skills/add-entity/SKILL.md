---
name: add-entity
description: Scaffold a new domain entity or aggregate in this Vertical Slice Architecture template — the entity itself, its strongly typed ID, errors, specification, EF Core configuration, DbSet, the mandatory Vogen registration, and the migration. Use when the user says "add an entity", "add an aggregate", "create a domain model", "add a new table", "model X in the domain", or names a new domain concept that needs to be persisted. Use it before `/add-slice` when the use case needs a domain type that doesn't exist yet.
---

# Add an Entity

A new persisted domain type in this template is never one file. It's a domain object, a strongly typed ID, an EF configuration, a `DbSet`, a converter registration, and a migration — spread across two folders. Miss one and you get anything from a compile error to an app that won't start.

## The trap this skill exists to close

**Every strongly typed ID must be registered in `src/WebApi/Common/Persistence/VogenEfCoreConverters.cs`.**

Forget it and nothing complains at compile time. `dotnet build` is green, the architecture tests are green, and the app throws on startup when EF Core tries to map a `HeroId` it has no converter for. That is the single most expensive thing to forget in this repo, so step 5 below is not optional and the verification step checks for it explicitly.

## Read the live reference first

Before generating anything, read the canonical aggregate and its persistence wiring:

- `src/WebApi/Common/Domain/Heroes/Hero.cs` — aggregate root, Vogen ID, `field`-keyword setter guards
- `src/WebApi/Common/Domain/Teams/Team.cs` — an aggregate that returns `ErrorOr<Success>` from behaviour
- `src/WebApi/Common/Persistence/Heroes/HeroConfiguration.cs` — EF configuration
- `src/WebApi/Common/Persistence/VogenEfCoreConverters.cs` — the registration list

The templates in `references/` follow these files, but the repo is the source of truth. If the two disagree, the repo wins — follow it and update the template (see *Keeping this skill honest* at the bottom).

One deliberate divergence: the aggregate template gives properties a `private set` plus a named mutator, following `Team.cs`. `Hero.cs` uses a public `set` instead, so `UpdateHeroEndpoint` can assign `hero.Name` directly. Both keep the guard in the setter, which is the part that matters; prefer the `Team.cs` shape for new work, because it keeps the aggregate in charge of how it changes.

## What you need to know before scaffolding

Ask the user for anything not already stated:

| Input | Notes |
|---|---|
| Entity name | Singular, PascalCase (`Hero`, `Mission`). The folder and pluralised `DbSet` derive from it. |
| Aggregate or child entity? | Aggregate root if it's the transactional boundary or raises domain events. Child entity if it only ever lives inside another aggregate (`Mission` inside `Team`). |
| Owning aggregate | For a child entity — which aggregate holds it, and via what collection. |
| Properties | Name, type, required/optional, and a max length for every string. |
| Behaviour | The methods that mutate it. Guards go in the property setter, business rules go in the method. |
| Domain events | Does any mutation need to trigger work elsewhere? Aggregate roots only. |

If the entity has no behaviour and no invariants, say so — it may want to be a value object (a `record` implementing `IValueObject`, like `Power`) rather than an entity. Value objects need no ID, no configuration entry of their own, and no migration when owned.

## Steps

Work in this order. Later steps depend on earlier ones compiling.

1. **Domain folder** — create `src/WebApi/Common/Domain/{Aggregate}/` and add the entity plus its `[ValueObject<Guid>]` ID. Template: [references/domain.md](references/domain.md).
2. **Errors** — `{Entity}Errors.cs` with `Error` constants. Add `NotFound` at minimum; endpoints will reach for it.
3. **Specification** — `{Entity}Spec.cs` extending `SingleResultSpecification<T>`, with a static `ById(...)` factory. One spec class per aggregate; add a factory method per query rather than a new class. Child entities don't get their own spec — they're loaded through the aggregate's.
4. **EF configuration** — `src/WebApi/Common/Persistence/{Aggregate}/{Entity}Configuration.cs` inheriting `AuditableConfiguration<T>`. Every string property's `HasMaxLength` reads the entity's `const`, never a literal. Template: [references/persistence.md](references/persistence.md).
5. **Vogen registration** — add `[EfCoreConverter<{Entity}Id>]` to `VogenEfCoreConverters`. See the trap above.
6. **DbSet** — for an aggregate root only, add `ApplicationDbContext.{Entities}.cs` exposing `DbSet<{Entity}>` via `AggregateRootSet<T>()`. Child entities are reached through their aggregate, so they get no `DbSet`.
7. **Migration** — run it yourself, don't just print the command:

   ```bash
   dotnet ef migrations add Add{Entity} \
     --project src/WebApi/WebApi.csproj \
     --startup-project src/WebApi/WebApi.csproj \
     --output-dir Common/Persistence/Migrations
   ```

   Then **read the generated migration** and confirm the tables and columns match what you configured. An empty `Up()` means EF didn't see your entity — usually a missing `DbSet` or a configuration that wasn't picked up by `ApplyConfigurationsFromAssembly`.

8. **Test data** — add a Bogus factory at `tests/WebApi.IntegrationTests/Common/Factories/{Entity}Factory.cs` so integration tests have something to seed. Template in [references/persistence.md](references/persistence.md).
9. **Seeding (optional)** — extend `tools/Seeder/Initializers/ApplicationDbContextInitializer.cs` if the entity should show up in dev. Keep it idempotent: short-circuit when rows already exist.
10. **Unit tests** — invariants and factory rules belong in `tests/WebApi.UnitTests/Features/{Aggregate}/{Entity}Tests.cs`. Cover the guards you wrote: a null/blank string should throw, an over-long string should throw, and the happy path should succeed.

## Verification

```bash
dotnet build && dotnet build -c Release
dotnet test tests/WebApi.UnitTests
```

Then confirm the two things a build can't:

- **Vogen registration** — `grep EfCoreConverter src/WebApi/Common/Persistence/VogenEfCoreConverters.cs` lists your new ID. A green build proves nothing here.
- **The app still starts** — the registration failure only surfaces at runtime, so boot it: `aspire start --isolated` (see the `aspire` skill), wait for the WebApi resource to report healthy, and check the migration ran. Alternatively `dotnet test tests/WebApi.IntegrationTests` exercises real startup against a real SQL Server, which catches the same class of failure.

Full detail on what "done" means: [`.claude/rules/verification.md`](../../rules/verification.md).

## Guardrails

- **Don't put guards in the factory.** They go in the property setter using the `field` keyword — the setter is the only path every assignment goes through, including EF materialisation and later mutation.
- **Don't add a public parameterless constructor.** EF needs a `private` one; the architecture tests assert this.
- **Don't reference a feature slice from the domain.** Domain types know nothing about `Features/`.
- **Don't hand-edit a migration** to fix a modelling mistake. Fix the entity or configuration and regenerate.
- **Don't delete or edit existing migrations.** Roll forward with a new one.

## Keeping this skill honest

The templates in `references/` are copies of the repo's shapes, so they drift when the repo changes. If you hit a place where a template no longer matches `Hero.cs` or `HeroConfiguration.cs`, update the template as part of the same change.
