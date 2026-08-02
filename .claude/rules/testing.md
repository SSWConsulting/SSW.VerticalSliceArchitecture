---
paths:
  - "tests/**/*"
---

# Testing

Three test projects, three different jobs.

## Unit Tests — `tests/WebApi.UnitTests/`

Anything that runs without infrastructure. No EF, no mocks.

- **Domain logic** — entity invariants, value objects, factory rules. Reference: `tests/WebApi.UnitTests/Features/Heroes/HeroTests.cs`.
- **Request validators** — construct the validator directly (`new CreateHeroRequestValidator().Validate(req)`) and assert on `IsValid` / `Errors`. No DI or test host is needed, provided the rules don't call `Resolve<T>()`. Reference: `tests/WebApi.UnitTests/Features/Heroes/CreateHeroRequestValidatorTests.cs`.

Where a validator mirrors a domain limit, drive the test boundaries off the domain constant (`Hero.NameMaxLength`) rather than a literal, so the test follows the limit when it moves.

## Integration Tests — `tests/WebApi.IntegrationTests/`

- Inherit `IntegrationTestBase` to get the shared `TestingDatabaseFixture` (real SQL Server via Testcontainers, reset between tests with Respawn).
- `GetAnonymousClient()` for HTTP, `GetQueryable<T>()` for read-only EF assertions, `AddAsync(entity)` to seed test data.
- Reference: `tests/WebApi.IntegrationTests/Endpoints/Heroes/Commands/CreateHeroCommandTests.cs`.
- Fast despite hitting a real database, because Respawn truncates rather than recreating.

## Architecture Tests — `tests/WebApi.ArchitectureTests/`

Enforces naming and layering rules. A failure here means a convention has been broken; fix the code, not the test.

`FeatureTests` covers the slice conventions:

- every endpoint is named `*Endpoint` and lives in a `Features.{Feature}.{Slice}` namespace
- every endpoint with a request has a FastEndpoints `Validator<TRequest>` **in the same slice namespace** (`EndpointWithoutRequest` is exempt; a plain FluentValidation `AbstractValidator<T>` does not count, because FastEndpoints never binds one)
- every endpoint whose request derives from `PagedRequest` has a `PagedRequestValidator<TRequest, TEntity>` in that namespace, not merely some `Validator<TRequest>` — the sort allow-list lives in that base class, and without it an unknown sort column reaches the primitives, which throw rather than return a 400
- no slice depends on another slice's types
- endpoints take `ApplicationDbContext`, not the `DbContext` base type — checked against IL, so `Resolve<T>()` and handler-method injection are covered too

`DomainTests` covers the domain conventions: entities and value objects inherit the right base types, and entities have a private parameterless constructor for EF.

Every test guards its match set with `Should().NotBeEmpty()` first. Without that, a filter that stops matching — a renamed namespace, a dropped interface — turns the test green instead of red, and it silently stops enforcing anything. Copy the guard into any new rule.

## Running

```bash
dotnet test                                  # all
dotnet test tests/WebApi.IntegrationTests/   # one project
```
