# Test templates for a slice

Two kinds, in two projects. Reference:
`tests/WebApi.IntegrationTests/Features/Heroes/Mutations/CreateHeroMutationTests.cs` and
`tests/WebApi.UnitTests/Features/Heroes/CreateHeroInputValidatorTests.cs`.

---

## Integration test

`tests/WebApi.IntegrationTests/Features/{Feature}/{Mutations|Queries}/{UseCase}{Mutation|Query}Tests.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.{Feature}.Mutations;

public class {UseCase}MutationTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Document =
        """
        mutation {UseCase}($input: {UseCase}Input!) {
          {useCase}(input: $input) {
            {aggregate} { id name }
            errors { __typename ... on Error { message } }
          }
        }
        """;

    [Fact]
    public async Task Mutation_Should{DoTheThing}()
    {
        // Arrange
        var {aggregate} = {Aggregate}Factory.Generate();
        await AddAsync({aggregate});

        var input = new { {aggregate}Id = {aggregate}.Id.Value.ToString(), name = "New name" };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var payload = result.Field("{useCase}");
        payload.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Null, result.RawBody);

        var updated = await GetQueryable<{Aggregate}>()
            .FirstAsync(x => x.Id == {aggregate}.Id, CancellationToken);
        updated.Name.Should().Be("New name");
    }

    [Fact]
    public async Task Mutation_ShouldReturnNotFound_WhenThe{Aggregate}DoesNotExist()
    {
        // Arrange
        var input = new { {aggregate}Id = Guid.CreateVersion7().ToString(), name = "New name" };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var errors = result.Field("{useCase}").GetProperty("errors").EnumerateArray().ToList();
        errors.Should().ContainSingle();
        errors[0].GetProperty("__typename").GetString().Should().Be("NotFoundError");
        errors[0].GetProperty("message").GetString().Should().Be({Aggregate}Errors.NotFound.Description);
    }
}
```

What the shape is doing:

- **The document is written out in full.** It is the same text a client sends, so a renamed field
  fails the test instead of passing against a regenerated wrapper.
- **Assert on the payload, not the status code.** GraphQL answers a failed request with HTTP 200 and
  an `errors` array. `result.Field(name)` fails with the whole body when the request itself errored;
  a business failure is data inside the payload, so check `errors[0].__typename`.
- **`errors` is null on the success path.** Assert that too — a mutation that half worked would
  otherwise pass.
- **Read the result back through EF.** The payload says what the resolver returned; `GetQueryable<T>`
  says what was saved.
- **Variables are anonymous objects**, serialised camelCase. A strongly typed id goes over the wire
  as a string: `{aggregate}.Id.Value.ToString()`.

For a query, assert on the connection: `nodes`, `totalCount` and `pageInfo`. For a field resolved by
a DataLoader, ask for it in the document — that is the only thing that proves the loader runs.

---

## Validator unit test

`tests/WebApi.UnitTests/Features/{Feature}/{UseCase}InputValidatorTests.cs`

```csharp
using SSW.VerticalSliceArchitecture.Domain.{Aggregate};
using SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.{Feature};

public class {UseCase}InputValidatorTests
{
    private readonly {UseCase}InputValidator _validator = new();

    // The domain setters throw on over-length input, so anything the validator lets through
    // faults the resolver instead of returning a typed error. These boundaries are the contract
    // between the two.
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData({Aggregate}.NameMaxLength, true)]
    [InlineData({Aggregate}.NameMaxLength + 1, false)]
    public void Validator_WithNameOfLength_ShouldMatchDomainLimit(int length, bool expectedValid)
    {
        // Arrange
        var input = CreateInput(name: new string('a', length));

        // Act
        var result = _validator.Validate(input);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    private static {UseCase}Input CreateInput(
        {Aggregate}Id? {aggregate}Id = null,
        string name = "A name") =>
        new({aggregate}Id ?? {Aggregate}Id.From(Guid.CreateVersion7()), name);
}
```

Drive the boundaries off the domain constant rather than a literal, so the test follows the limit
when it moves. No DI and no test host: the validator is a plain object.

---

## Running

```bash
dotnet test tests/WebApi.UnitTests
dotnet test tests/WebApi.IntegrationTests    # needs Docker or Podman running
```

After a slice is added the schema snapshot test fails until you regenerate it:

```bash
dotnet run --project src/WebApi -- schema export --output src/WebApi/schema.graphql
```
