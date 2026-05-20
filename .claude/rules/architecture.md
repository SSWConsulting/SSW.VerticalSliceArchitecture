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
    {UseCase}Response.cs
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

## Error Handling

- Validation runs before `HandleAsync` and auto-returns 400.
- `ThrowError("message")` for ad-hoc validation failures inside the handler.
- Eventual-consistency failures inside domain event handlers: throw `EventualConsistencyException`. `EventualConsistencyMiddleware` translates it into the right HTTP response.
- Global exception handler covers anything else.

## Gotchas

- Forgetting `Group<TGroup>()` → endpoint registers without the feature's prefix.
- Forgetting to register a new strongly typed ID in `VogenEfCoreConverters` → app fails at startup. See [database.md](database.md).
- Loading aggregates without an Ardalis spec → silently missing related data. See [domain.md](domain.md).
