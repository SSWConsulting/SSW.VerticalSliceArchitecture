using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Common.Pagination;
using SSW.VerticalSliceArchitecture.Features.Teams.GetAllTeams;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.Teams.Queries;

public class GetAllTeamsQueryTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Route = "/api/teams";

    [Fact]
    public async Task Query_ShouldReturnFirstPage_WhenPagingIsNotSpecified()
    {
        // Arrange
        const int entityCount = 25;
        await AddRangeAsync(TeamFactory.Generate(entityCount));

        // Act
        var page = await GetPage(Route);

        // Assert
        page.Items.Should().HaveCount(PagingParams.DefaultPageSize);
        page.TotalCount.Should().Be(entityCount);
        page.HasNextPage.Should().BeTrue();

        var firstTeam = page.Items[0];
        firstTeam.Id.Should().NotBeEmpty();
        firstTeam.Name.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Query_ShouldReturnRequestedPage()
    {
        // Arrange
        await AddRangeAsync(TeamFactory.Generate(10));
        var expected = await SortedNames();

        // Act
        var page = await GetPage($"{Route}?page=2&pageSize=4");

        // Assert
        page.Page.Should().Be(2);
        page.TotalCount.Should().Be(10);
        page.Items.Select(t => t.Name).Should().Equal(expected.Skip(4).Take(4));
    }

    [Fact]
    public async Task Query_ShouldSortDescending()
    {
        // Arrange
        await AddRangeAsync(TeamFactory.Generate(5));
        var expected = await SortedNames();

        // Act
        var page = await GetPage($"{Route}?pageSize=5&sortBy=name&sortDirection=desc");

        // Assert
        page.Items.Select(t => t.Name).Should().Equal(expected.Reverse());
    }

    [Fact]
    public async Task Query_ShouldClampPageSize_WhenAboveTheMaximum()
    {
        // Arrange
        await AddRangeAsync(TeamFactory.Generate(5));

        // Act
        var page = await GetPage($"{Route}?pageSize=5000");

        // Assert
        page.PageSize.Should().Be(PagingParams.MaxPageSize);
    }

    [Fact]
    public async Task Query_ShouldReturnBadRequest_WhenSortColumnIsNotAllowed()
    {
        // Arrange
        await AddRangeAsync(TeamFactory.Generate(3));

        // Act
        var response = await GetAnonymousClient().GetAsync($"{Route}?sortBy=notAColumn", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync(CancellationToken);
        body.Should().Contain("not a sortable column");
    }

    private async Task<PagedList<GetAllTeamsResponse>> GetPage(string url)
    {
        var response = await GetAnonymousClient().GetAsync(url, CancellationToken);

        response.IsSuccessStatusCode.Should().BeTrue();

        var page = await response.Content.ReadFromJsonAsync<PagedList<GetAllTeamsResponse>>(CancellationToken);
        return page.Should().NotBeNull().And.Subject.As<PagedList<GetAllTeamsResponse>>();
    }

    /// <remarks>
    /// Expected order comes back out of the database rather than a hard-coded list, because the teams are
    /// randomly generated. Ordering mirrors the spec — sort column then <c>Id</c> as tie-breaker.
    /// </remarks>
    private async Task<string[]> SortedNames() =>
        await GetQueryable<Team>()
            .OrderBy(t => t.Name)
            .ThenBy(t => t.Id)
            .Select(t => t.Name)
            .ToArrayAsync(CancellationToken);
}
