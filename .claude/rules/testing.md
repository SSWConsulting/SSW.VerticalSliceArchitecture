---
paths:
  - "tests/**/*"
---

# Testing

Three test projects, three different jobs.

## Unit Tests — `tests/WebApi.UnitTests/`

Anything that runs without infrastructure. No EF, no mocks.

- **Domain logic** — entity invariants, value objects, factory rules. Reference: `tests/WebApi.UnitTests/Domain/Heroes/HeroTests.cs`.
- **Input validators** — construct the validator directly (`new CreateHeroInputValidator().Validate(input)`) and assert on `IsValid` / `Errors`. No DI or test host is needed. Reference: `tests/WebApi.UnitTests/Features/Heroes/CreateHeroInputValidatorTests.cs`.

Where a validator mirrors a domain limit, drive the test boundaries off the domain constant (`Hero.NameMaxLength`) rather than a literal, so the test follows the limit when it moves.

## Integration Tests — `tests/WebApi.IntegrationTests/`

- Inherit `IntegrationTestBase` to get the shared `TestingDatabaseFixture` (real SQL Server via Testcontainers, reset between tests with Respawn).
- `ExecuteAsync(document, variables)` posts a GraphQL document, `GetQueryable<T>()` reads back with EF for assertions, `AddAsync(entity)` seeds test data.
- Write the document out in full in the test. It is the same text a client would send, so a renamed
  field fails a test rather than passing against a regenerated wrapper.
- `GraphQlResult.Field(name)` fails with the whole response body when the request returned errors.
  GraphQL answers with HTTP 200 and an `errors` array, so a test that checked only the status code
  would pass on a failed query.
- A mutation's business failures are data, not transport failures: assert on
  `payload.errors[0].__typename`, and on `errors` being null for the success path.
- Reference: `tests/WebApi.IntegrationTests/Features/Heroes/Mutations/CreateHeroMutationTests.cs`.
- Fast despite hitting a real database, because Respawn truncates rather than recreating.

### The schema snapshot

`Schema/SchemaSnapshotTests.cs` compares the schema the server builds with the checked-in
`src/WebApi/schema.graphql`. A GraphQL schema is a published contract, and almost every resolver
signature change edits it, so this puts the change in the diff. To accept one:

```bash
dotnet run --project src/WebApi -- schema export --output src/WebApi/schema.graphql
```

## Architecture Tests — `tests/WebApi.ArchitectureTests/`

Enforces naming and layering rules. A failure here means a convention has been broken; fix the code, not the test.

`FeatureTests` covers the slice conventions:

- every resolver is named `*Query`, `*Mutation` or `*Subscription` and lives in a `Features.{Feature}.{Slice}` namespace
- every resolver is a static class, because the source generator emits the other half of a static partial and a non-static one contributes no field at all
- every mutation input has an `AbstractValidator<TInput>` **in the same slice namespace** — GraphQL validates the shape of an input, never its content
- no slice depends on another slice's types
- resolvers take `ApplicationDbContext`, not the `DbContext` base type — checked against IL, so a context resolved from the service provider is covered too
- an `[ObjectType<T>]` sits at feature level, never inside a slice: one Hero type serves every hero slice

`DomainTests` covers the domain conventions: entities and value objects inherit the right base types, entities have a private parameterless constructor for EF, and the Domain project references nothing but Ardalis.Specification, ErrorOr and Vogen.

Every test guards its match set with `Should().NotBeEmpty()` first. Without that, a filter that stops matching — a renamed namespace, a dropped interface — turns the test green instead of red, and it silently stops enforcing anything. Copy the guard into any new rule.

## Running

```bash
dotnet test                                  # all
dotnet test tests/WebApi.IntegrationTests/   # one project
```
