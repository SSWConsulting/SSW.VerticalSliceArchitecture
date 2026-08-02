using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SSW.VerticalSliceArchitecture.Common.Pagination;
using SSW.VerticalSliceArchitecture.Common.Persistence;

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

    /// <summary>
    /// GETs a paged list endpoint by raw URL and deserialises the standard paged envelope.
    /// </summary>
    /// <remarks>
    /// Takes a URL rather than a request DTO on purpose: what a paged endpoint's tests pin down is the
    /// query-string contract — the parameter names and how out-of-range values are treated — and a typed
    /// helper would go around it.
    /// </remarks>
    protected async Task<PagedList<T>> GetPage<T>(string url)
    {
        var response = await GetAnonymousClient().GetAsync(url, CancellationToken);

        response.IsSuccessStatusCode.Should().BeTrue();

        var page = await response.Content.ReadFromJsonAsync<PagedList<T>>(CancellationToken);
        return page.Should().NotBeNull().And.Subject.As<PagedList<T>>();
    }

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        return ValueTask.CompletedTask;
    }
}