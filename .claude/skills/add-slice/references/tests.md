# Test templates

Three projects, three jobs — see [`.claude/rules/testing.md`](../../../rules/testing.md). A new slice earns an integration test; new domain behaviour earns a unit test. The architecture tests need nothing added, they just have to stay green.

Placeholders as in [command-slice.md](command-slice.md).

---

## Integration test — command

`tests/WebApi.IntegrationTests/Endpoints/{Feature}/Commands/{UseCase}CommandTests.cs`

```csharp
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};
using SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using System.Net;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.{Feature}.Commands;

public class {UseCase}CommandTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Command_Should{DoTheThing}()
    {
        // Arrange
        var cmd = new {UseCase}Request("Clark Kent");
        var client = GetAnonymousClient();

        // Act
        var result = await client.POSTAsync<{UseCase}Endpoint, {UseCase}Request, {UseCase}Response>(cmd);

        // Assert
        result.Response.StatusCode.Should().Be(HttpStatusCode.OK);

        var item = await GetQueryable<{Entity}>().FirstAsync(CancellationToken);
        item.Should().NotBeNull();
        item.Name.Should().Be(cmd.Name);
        item.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(10));
    }
}
```

What each piece is doing:

- `IntegrationTestBase(fixture)` joins the shared `TestingDatabaseFixture` — a real SQL Server via Testcontainers, truncated between tests by Respawn rather than recreated, which is why these stay fast.
- `POSTAsync<TEndpoint, TRequest, TResponse>` is FastEndpoints' typed client. Routing to the endpoint *type* means a route change can't silently leave the test passing against a stale URL.
- `GetQueryable<T>()` is an untracked read straight from the database, so the assertion sees what was actually persisted rather than what's sitting in the endpoint's change tracker.
- `CancellationToken` comes from the base class (`TestContext.Current.CancellationToken`).
- `Xunit`, `AwesomeAssertions` and `Bogus` are global usings in this project.

For a 204 command, the typed call drops the response type:

```csharp
var result = await client.POSTAsync<{UseCase}Endpoint, {UseCase}Request>(cmd);
result.StatusCode.Should().Be(HttpStatusCode.NoContent);
```

Note the shape difference — the two-generic overload returns the `HttpResponseMessage` itself, so it's `result.StatusCode`, not `result.Response.StatusCode`.

### Cover the failure path too

A command with a 404 or a domain-error path needs a test for it. The success case alone will pass against an endpoint that swallowed the not-found branch.

```csharp
[Fact]
public async Task Command_WhenNotFound_ShouldReturn404()
{
    // Arrange
    var cmd = new {UseCase}Request(Guid.CreateVersion7(), "New name");
    var client = GetAnonymousClient();

    // Act
    var result = await client.PUTAsync<{UseCase}Endpoint, {UseCase}Request>(cmd);

    // Assert
    result.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

---

## Integration test — query

`tests/WebApi.IntegrationTests/Endpoints/{Feature}/Queries/{UseCase}QueryTests.cs`

```csharp
using System.Net;
using SSW.VerticalSliceArchitecture.Common.Pagination;
using SSW.VerticalSliceArchitecture.Features.{Feature}.{UseCase};
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.{Feature}.Queries;

public class {UseCase}QueryTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Route = "/api/{entities}";

    [Fact]
    public async Task Query_ShouldReturnFirstPage_WhenPagingIsNotSpecified()
    {
        // Arrange
        const int entityCount = 25;
        await AddRangeAsync({Entity}Factory.Generate(entityCount));

        // Act
        var page = await GetPage<{UseCase}Response>(Route);

        // Assert
        page.Items.Should().HaveCount(PagingParams.DefaultPageSize);
        page.TotalCount.Should().Be(entityCount);
        page.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Query_ShouldReturnBadRequest_WhenSortColumnIsNotAllowed()
    {
        // Arrange
        await AddRangeAsync({Entity}Factory.Generate(3));

        // Act
        var response = await GetAnonymousClient().GetAsync($"{Route}?sortBy=notAColumn", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

A paged endpoint's tests go through raw URLs and `GetPage<T>` on `IntegrationTestBase` rather than the typed FastEndpoints client, because what they pin down *is* the query-string contract — the parameter names and how out-of-range values are treated. A typed helper would go around the thing under test.

Cover the boundaries, not just the happy path: the default page, an explicit `page`/`pageSize`, a partial last page, a page past the end (empty `items`, correct `totalCount`), `pageSize` above the cap, both sort directions, and a 400 for an unknown sort column and direction. `GetAllHeroes` has the full set.

Seed through the Bogus factory (`tests/WebApi.IntegrationTests/Common/Factories/{Entity}Factory.cs` — template in [`add-entity/references/persistence.md`](../../add-entity/references/persistence.md)) rather than constructing entities inline, so a change to the entity's factory signature lands in one place.

`AddAsync` seeds one, `AddRangeAsync` seeds many. Both save immediately.

For a single-item query with a route parameter, the typed client is the right tool — there's no query string to pin down:

```csharp
var result = await client.GETAsync<{UseCase}Endpoint, {UseCase}Request, {UseCase}Response>(
    new {UseCase}Request(entity.Id.Value));
```

---

## Unit test — domain behaviour

`tests/WebApi.UnitTests/Features/{Aggregate}/{Entity}Tests.cs`

No EF, no mocks, no HTTP. Just the entity and its rules. If a test here needs a `DbContext`, the logic is in the wrong place.

```csharp
using SSW.VerticalSliceArchitecture.Common.Domain.{Aggregate};

namespace SSW.VerticalSliceArchitecture.UnitTests.Features.{Aggregate};

public class {Entity}Tests
{
    [Fact]
    public void Create_WithValidName_ShouldSucceed()
    {
        // Act
        var {entity} = {Entity}.Create("name");

        // Assert
        {entity}.Should().NotBeNull();
        {entity}.Name.Should().Be("name");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithBlankName_ShouldThrow(string? name)
    {
        // Act
        Action act = () => {Entity}.Create(name!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void {DoSomething}_WhenNotAvailable_ShouldReturnError()
    {
        // Arrange
        var {entity} = {Entity}.Create("name");
        {entity}.{DoSomething}("first");

        // Act
        var result = {entity}.{DoSomething}("second");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be({Entity}Errors.NotAvailable);
    }

    [Fact]
    public void {DoSomething}_ShouldRaise{Event}Event()
    {
        // Arrange
        var {entity} = {Entity}.Create("name");

        // Act
        {entity}.{DoSomething}("description");

        // Assert
        var domainEvents = {entity}.PopDomainEvents();
        domainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<{Event}Event>();
    }
}
```

Assert on the `Error` constant itself, not on its message string. The message is prose that will get reworded; the constant is the contract.

`PopDomainEvents()` drains the list, so call it once per assertion block.

---

## Architecture tests

`tests/WebApi.ArchitectureTests/` needs nothing added for a new slice — it asserts conventions across the whole assembly. A red test here means the new code broke a rule, so fix the code rather than the test. `DomainTests` is the one new entities trip: every domain type must inherit `Entity<T>` or `AggregateRoot<T>`, or implement `IEvent` or `IValueObject`, and every entity needs a private parameterless constructor.

---

## Running

```bash
dotnet test tests/WebApi.UnitTests           # fast, no infrastructure
dotnet test tests/WebApi.ArchitectureTests   # fast, no infrastructure
dotnet test tests/WebApi.IntegrationTests    # needs Docker or Podman running
dotnet test                                  # all three
```

When the integration tests fail to start their container rather than failing an assertion, the container runtime isn't up.
