using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSW.VerticalSliceArchitecture.Common.Persistence;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Infrastructure.GraphQL;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Common;

/// <summary>
/// Integration tests inherit from this to access helper classes
/// </summary>
[Collection<TestingDatabaseFixtureCollection>]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    private readonly TestingDatabaseFixture _fixture;
    private readonly ApplicationDbContext _dbContext;

    protected IntegrationTestBase(TestingDatabaseFixture fixture)
    {
        _fixture = fixture;
        _scope = _fixture.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// Setup for each test
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _fixture.TestSetup();
    }

    /// <summary>The API's services, for the rare test that needs one directly.</summary>
    protected IServiceProvider Services => _scope.ServiceProvider;

    protected IQueryable<T> GetQueryable<T>() where T : class => _dbContext.Set<T>().AsNoTracking();

    protected async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        await _dbContext.AddAsync(entity, CancellationToken);
        await _dbContext.SaveChangesAsync(CancellationToken);
    }

    protected async Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class
    {
        await _dbContext.AddRangeAsync(entities, CancellationToken);
        await _dbContext.SaveChangesAsync(CancellationToken);
    }

    protected async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync(CancellationToken);
    }

    protected HttpClient GetAnonymousClient() => _fixture.AnonymousClient.Value;

    /// <summary>Sends a GraphQL document to the API as an anonymous caller.</summary>
    protected Task<GraphQlResult> ExecuteAsync(string document, object? variables = null) =>
        new GraphQlClient(GetAnonymousClient()).ExecuteAsync(document, variables, CancellationToken);

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        return ValueTask.CompletedTask;
    }
}
