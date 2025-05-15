using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VerticalSliceArchitectureTemplate.Common.Persistence;

namespace VerticalSliceArchitectureTemplate.IntegrationTests.Common;

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

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        return ValueTask.CompletedTask;
    }
}