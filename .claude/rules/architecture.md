---
paths:
  - "src/WebApi/Features/**/*"
  - "src/WebApi/Common/Interfaces/**/*"
---

# Architecture — Vertical Slices + FastEndpoints

## Slice Layout

```
src/WebApi/Features/{Feature}/
  {Feature}Feature.cs    # IFeature.ConfigureServices — only used if the slice needs DI
  {Feature}Group.cs      # FastEndpoints Group — sets route prefix (becomes /api/{prefix})
  {UseCase}/             # one folder per use case
    {UseCase}Endpoint.cs
    {UseCase}Request.cs
    {UseCase}Response.cs   # for a list use case this is one item — see Paging & Sorting
    {UseCase}RequestValidator.cs
    {UseCase}Summary.cs
```

Reference slice: `src/WebApi/Features/Heroes/CreateHero/`. Copy its shape rather than reinventing.

## Conventions

- Namespace mirrors the folder: `Features.Heroes.CreateHero`.
- One endpoint per file. Request/Response/Validator/Summary each get their own file (they tend to grow).
- `Group<TGroup>()` in every endpoint's `Configure()`, otherwise the endpoint won't be grouped or prefixed.
- Endpoint names use `Description(x => x.WithName("CreateHero"))` so generated OpenAPI client method names stay clean.
- Use `await Send.OkAsync(...)`, `Send.CreatedAsync(...)`, `Send.NotFoundAsync(...)`. Don't `return` early without sending.

## Paging & Sorting

Every list endpoint pages. Returning a whole table is a bug waiting for the table to grow.

The primitives live in `src/WebApi/Common/Pagination/`. Reference slice: `Features/Heroes/GetAllHeroes/`.

- **Query parameters** — `page`, `pageSize`, `sortBy`, `sortDirection`. Inherit `PagedRequest` in the
  slice's request record so the names never drift between features.
- **Response** — `PagedList<{UseCase}Response>`, where `{UseCase}Response` is the *item*, not the list.
  The envelope carries `items`, `page`, `pageSize`, `totalCount`, `totalPages`, `hasPreviousPage`,
  `hasNextPage`.
- **Defaults and limits** — `page` 1, `pageSize` 10, capped at 100. Out-of-range values are **clamped**
  by `PagingParams.From(...)`, not rejected.
- **Sorting** — each aggregate spec owns a `SortColumnMap<T>` of the columns callers may sort by. An
  unknown column or direction is **rejected** with a 400: inherit `PagedRequestValidator<TRequest, TEntity>`
  in the slice's validator and pass the spec's map.
- **Querying** — `{Aggregate}Spec.Paged(paging, sortBy, sortDirection)` applies ordering, the `Id`
  tie-breaker, and `Skip`/`Take`; `dbContext.Set.ToPagedListAsync(spec, projection, ct)` runs the page and
  its count off the same spec and projects into the response DTO. The envelope's `page`/`pageSize` are read
  back off the spec, so the body can't describe a window the query didn't fetch — pass a spec built by
  anything other than a `Paged` factory and it throws rather than lying.
- **Swagger** — document the four parameters via `Params[nameof(...)]` in the slice's `Summary`, and use a
  `PagedList<…>` response example so the envelope shows up.

Clamp vs reject is deliberate: a page size over the cap has an obvious safe answer (the cap), while an
unknown sort column does not — falling back to a default would return a plausible page in the wrong order
and hide the caller's bug.

## Error Handling

- Validation runs before `HandleAsync` and auto-returns 400.
- `ThrowError("message")` for ad-hoc validation failures inside the handler.
- Eventual-consistency failures inside domain event handlers: throw `EventualConsistencyException`. `EventualConsistencyMiddleware` translates it into the right HTTP response.
- Global exception handler covers anything else.

## Gotchas

- Forgetting `Group<TGroup>()` → endpoint registers without the feature's prefix.
- Forgetting to register a new strongly typed ID in `VogenEfCoreConverters` → app fails at startup. See [database.md](database.md).
- Loading aggregates without an Ardalis spec → silently missing related data. See [domain.md](domain.md).
