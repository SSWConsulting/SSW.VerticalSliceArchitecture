---
name: add-feature
description: Scaffold a vertical slice in this Vertical Slice Architecture template — the FastEndpoints endpoint, request, response, validator, and summary for one use case, plus the feature group and the integration and unit tests. Use when the user says "add a feature", "add an endpoint", "add a slice", "add a command", "add a query", "expose X over the API", or names a new use case such as "let users archive a team". Run `/add-entity` first if the use case needs a domain type that doesn't exist yet.
---

# Add a Feature Slice

One use case, one folder. `CreateHero` is a slice; `Heroes` is the feature that groups slices. Getting that boundary right is most of what this skill does — the rest is filling in five small files that always look the same.

## Read the live reference first

- `src/WebApi/Features/Heroes/CreateHero/` — the canonical command slice, all five files
- `src/WebApi/Features/Heroes/GetAllHeroes/` — a list query with no request
- `src/WebApi/Features/Teams/GetTeam/` — a single-item query with a route parameter
- `src/WebApi/Features/Teams/AddHeroToTeam/` — a command that loads aggregates through specs

The templates in `references/` mirror these. If they disagree, the repo wins — follow it and fix the template.

## What you need to know before scaffolding

| Input | Notes |
|---|---|
| Feature | The plural noun that owns the route prefix (`Heroes`, `Teams`). New feature or existing? |
| Use case | Verb + noun, PascalCase (`CreateHero`, `ArchiveTeam`). This is the folder name, the endpoint name, and the OpenAPI operation ID. |
| Command or query | Command mutates and usually returns 200 with an ID or 204; query reads and projects. |
| HTTP verb and route | Relative to the group prefix — `Post("/")` becomes `POST /api/heroes`. |
| Request shape | Body fields plus any route parameters. FastEndpoints binds both into one request record. |
| Response shape | Or none, for a 204. |
| Failure modes | Not found, conflict, forbidden — each needs a `Produces(...)` and a matching `Send.*Async`. |

Ask about anything not stated. Guessing a route or a verb produces a slice that compiles and is wrong.

## Steps

1. **Feature scaffolding** — if the feature is new, create `src/WebApi/Features/{Feature}/` and add `{Feature}Group.cs`. Skip `{Feature}Feature.cs` unless the slice registers its own services; most don't need it, and an empty one is noise. Template: [references/command-slice.md](references/command-slice.md).
2. **Slice folder** — `src/WebApi/Features/{Feature}/{UseCase}/`, one folder per use case, namespace mirroring the folder.
3. **The five files** — endpoint, request, response, validator, summary. One type per file, even when a file is three lines; they grow. A query with no input skips the request and validator. A command returning 204 skips the response.
   - Command: [references/command-slice.md](references/command-slice.md)
   - Query: [references/query-slice.md](references/query-slice.md)
4. **Domain event handler** — only if the use case reacts to an event rather than serving a request. It lives in the consuming feature, not the domain: `src/WebApi/Features/{Feature}/{Event}/{Event}EventHandler.cs`.
5. **Tests** — an integration test per slice, plus unit tests for any domain behaviour the slice added. Template: [references/tests.md](references/tests.md).

## The checklist that catches the silent failures

Every one of these compiles fine when you get it wrong.

- **`Group<{Feature}Group>()` in `Configure()`.** Without it the endpoint registers at the root instead of under `/api/{prefix}`, and it loses the group's tag and its `ProducesProblemDetails(500)`.
- **`Description(x => x.WithName("{UseCase}"))`.** This is the OpenAPI operation ID, so it's what generated clients name the method. Missing, and callers get a mangled auto-generated name.
- **`await Send.*Async(...)`, never a bare `return`.** Returning from `HandleAsync` without sending produces a 200 with an empty body. Every path out of the handler ends in a `Send`.
- **`return` after `Send.NotFoundAsync(ct)`.** `Send` doesn't stop execution — the handler keeps running and will try to send twice.
- **A `Produces(...)` for every non-200 path you send.** The status code has to be in the OpenAPI document or clients won't handle it.
- **Business rules in the aggregate, not the endpoint.** If the handler contains an `if` about domain state, that check belongs on the entity, returning `ErrorOr<Success>`.
- **No `using` from another slice.** Two slices needing the same code means it belongs in `Common/`, or the domain, or duplicated. Slices don't import each other.
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

- **Don't add a `{Feature}Feature.cs` with an empty `ConfigureServices`.** Add it when the slice actually has services to register.
- **Don't put a DTO in a shared folder** so two slices can use it. Duplication between slices is the design, not a smell — it's what lets one slice change without breaking another.
- **Don't return `IActionResult` or use MVC attributes.** This is FastEndpoints; see [`docs/adr/20251018-api-use-fastendpoints-instead-of-minimal-apis.md`](../../../docs/adr/20251018-api-use-fastendpoints-instead-of-minimal-apis.md).
- **Don't validate inside `HandleAsync`** for anything the validator can express. The validator runs first and auto-returns 400. `ThrowError("...")` is for the ad-hoc case that needs handler context.
- **Don't touch another feature's folder.** If the use case needs data from another aggregate, load it through its spec — that's what `AddHeroToTeam` does.

## Keeping this skill honest

The `references/` templates are copies of the repo's shapes and will drift. When you find one that no longer matches the Heroes or Teams slices, update it as part of the same change.
