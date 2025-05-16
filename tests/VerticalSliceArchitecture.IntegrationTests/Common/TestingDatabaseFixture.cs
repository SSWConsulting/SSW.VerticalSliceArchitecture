using Microsoft.Extensions.DependencyInjection;
using VerticalSliceArchitectureTemplate.IntegrationTests.Common.Infrastructure.Database;
using VerticalSliceArchitectureTemplate.IntegrationTests.Common.Infrastructure.Web;

namespace VerticalSliceArchitectureTemplate.IntegrationTests.Common;

/// <summary>
/// Initializes and resets the database before and after each test. Shared across all integration tests.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class TestingDatabaseFixture : IAsyncLifetime
{
    private readonly TestDatabase _database = new();
    private WebApiTestFactory _factory = null!;
    private IServiceScopeFactory _scopeFactory = null!;

    /// <summary>
    /// Global setup for tests
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();
        _factory = new WebApiTestFactory(_database.DbConnection);
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Setup for each test
    /// </summary>
    public async Task TestSetup()
    {
        await _database.ResetAsync();
    }

    /// <summary>
    /// Global cleanup for tests
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _database.DisposeAsync();
        await _factory.DisposeAsync();
    }

    // NOTE: If you need an authenticated client, create a similar method that performance the authentication,
    // adds the appropriate headers and returns the authenticated client
    // For an example of this see https://github.com/SSWConsulting/Northwind365
    public Lazy<HttpClient> AnonymousClient => new(_factory.CreateClient());

    public IServiceScope CreateScope() => _scopeFactory.CreateScope();
}

[CollectionDefinition]
public class TestingDatabaseFixtureCollection : ICollectionFixture<TestingDatabaseFixture>;