---
paths:
  - "tests/**/*"
---

# Testing

Three test projects, three different jobs.

## Unit Tests — `tests/WebApi.UnitTests/`

Domain logic only: entity invariants, value objects, factory rules. No EF, no mocks. Reference: `tests/WebApi.UnitTests/Features/Heroes/HeroTests.cs`.

## Integration Tests — `tests/WebApi.IntegrationTests/`

- Inherit `IntegrationTestBase` to get the shared `TestingDatabaseFixture` (real SQL Server via Testcontainers, reset between tests with Respawn).
- `GetAnonymousClient()` for HTTP, `GetQueryable<T>()` for read-only EF assertions, `AddAsync(entity)` to seed test data.
- Reference: `tests/WebApi.IntegrationTests/Endpoints/Heroes/Commands/CreateHeroCommandTests.cs`.
- Fast despite hitting a real database, because Respawn truncates rather than recreating.

## Architecture Tests — `tests/WebApi.ArchitectureTests/`

Enforces naming and layering rules. A failure here means a convention has been broken; fix the code, not the test.

`FeatureTests` covers the slice conventions:

- every endpoint is named `*Endpoint` and lives in a `Features.{Feature}.{Slice}` namespace
- every endpoint with a request has a matching `Validator<TRequest>` (`EndpointWithoutRequest` is exempt)
- no slice depends on another slice's types
- endpoints take `ApplicationDbContext`, not the `DbContext` base type

`DomainTests` covers the domain conventions: entities and value objects inherit the right base types, and entities have a private parameterless constructor for EF.

Every test guards its match set with `Should().NotBeEmpty()` first. Without that, a filter that stops matching — a renamed namespace, a dropped interface — turns the test green instead of red, and it silently stops enforcing anything. Copy the guard into any new rule.

## Running

```bash
dotnet test                                  # all
dotnet test tests/WebApi.IntegrationTests/   # one project
```
