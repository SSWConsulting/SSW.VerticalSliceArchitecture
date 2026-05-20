---
paths:
  - "tests/**/*"
---

# Testing

Three test projects, three different jobs.

## Unit Tests — `tests/WebApi.UnitTests/`

Domain logic only: entity invariants, value objects, factory rules. No EF, no mocks. Reference: `Features/Heroes/HeroTests.cs`.

## Integration Tests — `tests/WebApi.IntegrationTests/`

- Inherit `IntegrationTestBase` to get the shared `TestingDatabaseFixture` (real SQL Server via Testcontainers, reset between tests with Respawn).
- `GetAnonymousClient()` for HTTP, `GetQueryable<T>()` for read-only EF assertions, `AddAsync(entity)` to seed test data.
- Reference: `Endpoints/Heroes/Commands/CreateHeroCommandTests.cs`.
- Fast despite hitting a real database, because Respawn truncates rather than recreating.

## Architecture Tests — `tests/WebApi.ArchitectureTests/`

Enforces naming and layering rules. A failure here means a convention has been broken; fix the code, not the test.

## Running

```bash
dotnet test                                  # all
dotnet test tests/WebApi.IntegrationTests/   # one project
```
