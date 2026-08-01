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

## Running

```bash
dotnet test                                  # all
dotnet test tests/WebApi.IntegrationTests/   # one project
```
