using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Pagination;
using SSW.VerticalSliceArchitecture.Features.Heroes.GetAllHeroes;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.Heroes.Queries;

/// <remarks>
/// Requests are built as raw URLs rather than through the typed FastEndpoints test helpers, because what
/// these tests are pinning down is the query-string contract itself — the parameter names and how
/// out-of-range values are handled.
/// </remarks>
public class GetAllHeroesQueryTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Route = "/api/heroes";

    [Fact]
    public async Task Query_ShouldReturnFirstPage_WhenPagingIsNotSpecified()
    {
        // Arrange
        const int entityCount = 25;
        await AddRangeAsync(HeroFactory.Generate(entityCount));

        // Act
        var page = await GetPage(Route);

        // Assert
        page.Items.Should().HaveCount(PagingParams.DefaultPageSize);
        page.Page.Should().Be(PagingParams.FirstPage);
        page.PageSize.Should().Be(PagingParams.DefaultPageSize);
        page.TotalCount.Should().Be(entityCount);
        page.TotalPages.Should().Be(3);
        page.HasPreviousPage.Should().BeFalse();
        page.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Query_ShouldReturnRequestedPage()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(10));
        var expected = await SortedAliases();

        // Act
        var page = await GetPage($"{Route}?page=2&pageSize=4&sortBy=alias");

        // Assert
        page.Page.Should().Be(2);
        page.PageSize.Should().Be(4);
        page.TotalCount.Should().Be(10);
        page.Items.Select(h => h.Alias).Should().Equal(expected.Skip(4).Take(4));
    }

    [Fact]
    public async Task Query_ShouldReturnPartialLastPage()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(10));

        // Act
        var page = await GetPage($"{Route}?page=3&pageSize=4");

        // Assert
        page.Items.Should().HaveCount(2);
        page.TotalCount.Should().Be(10);
        page.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Query_ShouldReturnNoItemsButTheTotalCount_WhenPageIsPastTheEnd()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(10));

        // Act
        var page = await GetPage($"{Route}?page=99&pageSize=10");

        // Assert
        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(10);
        page.Page.Should().Be(99);
    }

    [Fact]
    public async Task Query_ShouldClampPageSize_WhenAboveTheMaximum()
    {
        // Arrange — one more row than the cap, so a caller asking for everything still can't have it
        const int entityCount = PagingParams.MaxPageSize + 1;
        await AddRangeAsync(HeroFactory.Generate(entityCount));

        // Act
        var page = await GetPage($"{Route}?pageSize=5000");

        // Assert
        page.PageSize.Should().Be(PagingParams.MaxPageSize);
        page.Items.Should().HaveCount(PagingParams.MaxPageSize);
        page.TotalCount.Should().Be(entityCount);
    }

    [Fact]
    public async Task Query_ShouldClampPage_WhenBelowTheFirstPage()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(3));

        // Act
        var page = await GetPage($"{Route}?page=0");

        // Assert
        page.Page.Should().Be(PagingParams.FirstPage);
        page.Items.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    public async Task Query_ShouldSortByAnAllowedColumn(string sortDirection)
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(10));
        var expected = await SortedAliases();
        if (sortDirection == "desc")
            expected = [.. expected.Reverse()];

        // Act
        var page = await GetPage($"{Route}?pageSize=10&sortBy=alias&sortDirection={sortDirection}");

        // Assert
        page.Items.Select(h => h.Alias).Should().Equal(expected);
    }

    [Fact]
    public async Task Query_ShouldReturnBadRequest_WhenSortColumnIsNotAllowed()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(3));

        // Act
        var response = await GetAnonymousClient().GetAsync($"{Route}?sortBy=notAColumn", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync(CancellationToken);
        body.Should().Contain("not a sortable column");
    }

    [Fact]
    public async Task Query_ShouldReturnBadRequest_WhenSortDirectionIsNotAllowed()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(3));

        // Act
        var response = await GetAnonymousClient().GetAsync($"{Route}?sortBy=name&sortDirection=sideways", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync(CancellationToken);
        body.Should().Contain("not a valid sort direction");
    }

    [Fact]
    public async Task Query_ShouldReturnHeroPowers()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(1));

        // Act
        var page = await GetPage(Route);

        // Assert
        var hero = page.Items.Should().ContainSingle().Subject;
        hero.Id.Should().NotBeEmpty();
        hero.Name.Should().NotBeEmpty();
        hero.Powers.Should().NotBeEmpty();
    }

    private async Task<PagedList<GetAllHeroesResponse>> GetPage(string url)
    {
        var response = await GetAnonymousClient().GetAsync(url, CancellationToken);

        response.IsSuccessStatusCode.Should().BeTrue();

        var page = await response.Content.ReadFromJsonAsync<PagedList<GetAllHeroesResponse>>(CancellationToken);
        return page.Should().NotBeNull().And.Subject.As<PagedList<GetAllHeroesResponse>>();
    }

    /// <remarks>
    /// The expected order comes back out of the database rather than a hard-coded list, because the heroes
    /// are randomly generated. Ordering here mirrors the spec — sort column then <c>Id</c> as tie-breaker.
    /// </remarks>
    private async Task<string[]> SortedAliases() =>
        await GetQueryable<Hero>()
            .OrderBy(h => h.Alias)
            .ThenBy(h => h.Id)
            .Select(h => h.Alias)
            .ToArrayAsync(CancellationToken);
}
