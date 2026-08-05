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
        var page = await GetPage<GetAllHeroesResponse>(Route);

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
        var page = await GetPage<GetAllHeroesResponse>($"{Route}?page=2&pageSize=4&sortBy=alias");

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
        var page = await GetPage<GetAllHeroesResponse>($"{Route}?page=3&pageSize=4");

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
        var page = await GetPage<GetAllHeroesResponse>($"{Route}?page=99&pageSize=10");

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
        var page = await GetPage<GetAllHeroesResponse>($"{Route}?pageSize=5000");

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
        var page = await GetPage<GetAllHeroesResponse>($"{Route}?page=0");

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
        var page = await GetPage<GetAllHeroesResponse>($"{Route}?pageSize=10&sortBy=alias&sortDirection={sortDirection}");

        // Assert
        page.Items.Select(h => h.Alias).Should().Equal(expected);
    }

    // A numeric column's ordering lambda compiles to Convert(h.PowerLevel, Object), a different expression
    // shape from a string column's — so a string-only sorting suite says nothing about whether EF Core can
    // translate the numeric one, and a numeric column is the first thing a new map gains.
    [Theory]
    [InlineData("asc")]
    [InlineData("desc")]
    public async Task Query_ShouldSortByANumericColumn(string sortDirection)
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(10));
        var expected = await SortedPowerLevels();
        if (sortDirection == "desc")
            expected = [.. expected.Reverse()];

        // Act
        var page = await GetPage<GetAllHeroesResponse>(
            $"{Route}?pageSize=10&sortBy=powerLevel&sortDirection={sortDirection}");

        // Assert
        page.Items.Select(h => h.PowerLevel).Should().Equal(expected);
    }

    [Fact]
    public async Task Query_ShouldReturnNoItems_WhenPageIsFarEnoughPastTheEndToOverflow()
    {
        // Arrange — (page - 1) * pageSize overflows int, so this is the request that used to produce a
        // negative OFFSET rather than an empty page
        await AddRangeAsync(HeroFactory.Generate(3));

        // Act
        var page = await GetPage<GetAllHeroesResponse>($"{Route}?page={int.MaxValue}&pageSize=100");

        // Assert
        page.Items.Should().BeEmpty();
        page.TotalCount.Should().Be(3);
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
        var page = await GetPage<GetAllHeroesResponse>(Route);

        // Assert
        var hero = page.Items.Should().ContainSingle().Subject;
        hero.Id.Should().NotBeEmpty();
        hero.Name.Should().NotBeEmpty();
        hero.Powers.Should().NotBeEmpty();
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

    private async Task<int[]> SortedPowerLevels() =>
        await GetQueryable<Hero>()
            .OrderBy(h => h.PowerLevel)
            .ThenBy(h => h.Id)
            .Select(h => h.PowerLevel)
            .ToArrayAsync(CancellationToken);
}
