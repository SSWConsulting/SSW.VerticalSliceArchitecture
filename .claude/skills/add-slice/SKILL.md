---
name: add-slice
description: Scaffold a slice in this Vertical Slice Architecture template — one use case in its own folder, with its FastEndpoints endpoint, request, response, validator, and summary, plus the Feature and Group if they don't exist yet, plus tests. Use when the user says "add a slice", "add a use case", "add an endpoint", "add a command", "add a query", "expose X over the API", or names a use case such as "let users archive a team". Run `/add-entity` first if the use case needs a domain type that doesn't exist yet.
---

# Add a Slice

A **slice** is one use case, owning everything it needs from HTTP surface to persistence, in its own folder. `CreateHero` is a slice. A **Feature** is the group of slices over the same aggregate, sharing a route prefix — `Heroes` is a Feature, and it is not itself a slice. [`CONTEXT.md`](../../../CONTEXT.md) at the repo root defines both, along with Group, Endpoint, and the rest of the vocabulary. Use those words, and avoid the ones it lists under *Avoid*.

This skill adds a slice. It creates the Feature and its Group too, but only when the slice is the first one in that Feature.

Not every slice is HTTP. `PowerLevelUpdated` is a slice triggered by a domain event rather than a request — the same folder-per-use-case shape, with an event handler in place of an endpoint.

## Read the live reference first

- `src/WebApi/Features/Heroes/CreateHero/` — the canonical command slice, all five files
- `src/WebApi/Features/Heroes/GetAllHeroes/` — a paged, sorted list query
- `src/WebApi/Features/Teams/GetTeam/` — a single-item query with a route parameter
- `src/WebApi/Features/Teams/AddHeroToTeam/` — a command that loads aggregates through specs
- `src/WebApi/Features/Teams/PowerLevelUpdated/` — an event-triggered slice

The templates in `references/` follow these. If they disagree, the repo wins — follow it and fix the template.

## What you need to know before scaffolding

| Input | Notes |
|---|---|
| Feature | The plural noun that owns the route prefix (`Heroes`, `Teams`). Does one already exist for this aggregate, or is this its first slice? |
| Use case | Verb + noun, PascalCase (`CreateHero`, `ArchiveTeam`). This names the slice folder, the endpoint, and the OpenAPI operation ID. |
| Command or query | A command mutates and returns 200 with an ID or 204; a query reads and projects. |
| HTTP verb and route | Relative to the Group prefix — `Post("/")` becomes `POST /api/heroes`. |
| Request shape | Body fields plus any route parameters. FastEndpoints binds both into one request record. |
| Response shape | Or none, for a 204. |
| Failure modes | Not found, conflict, forbidden — each needs a `Produces(...)` and a matching `Send.*Async`. |

Ask about anything not stated. Guessing a route or a verb produces a slice that compiles and is wrong.

## Steps

1. **Feature scaffolding** — only when this is the Feature's first slice: create `src/WebApi/Features/{Feature}/` and add `{Feature}Group.cs`. Skip `{Feature}Feature.cs` unless the Feature registers its own services; most don't, and an empty one is noise. Template: [references/command-slice.md](references/command-slice.md).
2. **Slice folder** — `src/WebApi/Features/{Feature}/{UseCase}/`, namespace mirroring the folder. The endpoint has to sit exactly two segments below `Features` — one for the Feature, one for the use case — or the architecture tests fail.
3. **The five files** — endpoint, request, response, validator, summary. One type per file, even when a file is three lines; they grow. A query with no input skips the request and validator; a list query is never one of those, because every list endpoint takes paging and sorting parameters. A command returning 204 skips the response.
   - Command: [references/command-slice.md](references/command-slice.md)
   - Query: [references/query-slice.md](references/query-slice.md)
4. **Event-triggered slice** — when the use case reacts to a domain event rather than a request, the slice holds an `IEventHandler<TEvent>` instead of an endpoint: `src/WebApi/Features/{Feature}/{Event}/{Event}EventHandler.cs`. It belongs to the Feature that *consumes* the event, not the one that raises it.
5. **Tests** — an integration test per slice, plus unit tests for any domain behaviour the slice added. Template: [references/tests.md](references/tests.md).

## What CI enforces

`tests/WebApi.ArchitectureTests/FeatureTests.cs` turns five of these into build failures rather than review comments:

- **Endpoints are named `*Endpoint` and live in a slice namespace** — exactly two segments below `Features`. A slice folder nested deeper or shallower fails.
- **Every endpoint with a request has a `Validator<TRequest>` in the same slice.** Not a shared one and not a bare `AbstractValidator` — FastEndpoints only binds validators derived from its own `Validator<T>`, and the test matches that base. An endpoint whose request type can't be read fails too, so derive from `Endpoint<TRequest, TResponse>` or one of its aliases.
- **A `PagedRequest` needs a `PagedRequestValidator<TRequest, TEntity>`**, not merely some `Validator<TRequest>`. The sort allow-list rules live in that base class, and the primitives throw on an unknown sort column rather than returning a 400 — so a bare validator would turn a documented 400 into a 500 with every other test still green.
- **No slice depends on another slice.** This is the rule that makes it Vertical Slice Architecture rather than layers in disguise.
- **Endpoints take `ApplicationDbContext`** — not the `DbContext` base type, and not a second `DbContext`. The check reads IL, so `Resolve<T>()` and handler-method injection are caught as well as constructor parameters.

Run them with `dotnet test tests/WebApi.ArchitectureTests`. A failure here means the code broke a rule; fix the code, not the test.

## The checklist that catches the silent failures

These all compile, and no test catches them.

- **`Group<{Feature}Group>()` in `Configure()`.** Without it the endpoint registers at the root instead of under `/api/{prefix}`, and it loses the Group's tag and its `ProducesProblemDetails(500)`.
- **`Description(x => x.WithName("{UseCase}"))`.** This is the OpenAPI operation ID, so it's what generated clients name the method. Missing, and callers get a mangled auto-generated name.
- **`await Send.*Async(...)`, never a bare `return`.** Returning from `HandleAsync` without sending produces a 200 with an empty body. Every path out ends in a `Send`.
- **`return` after `Send.NotFoundAsync(ct)`.** `Send` doesn't stop execution — `HandleAsync` carries on and will try to send twice.
- **A `Produces(...)` for every non-200 path you send.** The status code has to be in the OpenAPI document or clients won't handle it.
- **Business rules in the aggregate, not the endpoint.** If `HandleAsync` contains an `if` about domain state, that check belongs on the entity, returning `ErrorOr<Success>`.
- **Load the child collections before mutating.** Only `HasMany` navigations need this — owned collections (`OwnsMany(...).ToJson()`, like `Hero.Powers`) always come with their parent. For a `HasMany`, either call a spec factory that declares the `Include`s (`TeamSpec.ById` does; `HeroSpec.ById` declares none) or `.Include(...)` explicitly. With neither, the children arrive silently empty.

## Verification

```bash
dotnet build && dotnet build -c Release
dotnet test tests/WebApi.UnitTests tests/WebApi.ArchitectureTests
dotnet test tests/WebApi.IntegrationTests    # needs Docker or Podman running
```

Then exercise it for real — a green test suite doesn't prove the route is where you think it is. Boot with `aspire start --isolated` (see the `aspire` skill), wait for the WebApi resource to go healthy, and call the endpoint through `https://localhost:7255/swagger` or `curl`. Confirm the success path *and* at least one failure path, and check the route appears under the expected prefix.

Full detail: [`.claude/rules/verification.md`](../../rules/verification.md).

## Guardrails

- **Don't add a `{Feature}Feature.cs` with an empty `ConfigureServices`.** Add it when the Feature actually has services to register.
- **Don't put a DTO in a shared folder** so two slices can use it. Duplication between slices is the design, not a smell — it's what lets one slice change without breaking another, and `Slices_Should_NotDependOnOtherSlices` enforces it.
- **Don't return `IActionResult` or use MVC attributes.** This is FastEndpoints; see [`docs/adr/20251018-api-use-fastendpoints-instead-of-minimal-apis.md`](../../../docs/adr/20251018-api-use-fastendpoints-instead-of-minimal-apis.md).
- **Don't validate inside `HandleAsync`** for anything the validator can express. The validator runs first and auto-returns 400. `ThrowError("...")` is for the ad-hoc case that needs context only `HandleAsync` has.
- **Don't touch another Feature's folder.** If the use case needs data from another aggregate, load it through that aggregate's spec — that's what `AddHeroToTeam` does.

## Keeping this skill honest

The `references/` templates are copies of the repo's shapes and will drift. When you find one that no longer matches the Heroes or Teams slices, update it as part of the same change.
