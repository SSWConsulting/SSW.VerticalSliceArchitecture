using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Teams.Queries;

public class GetAllTeamsQueryTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const int DefaultPageSize = 10;

    [Fact]
    public async Task Query_ShouldReturnTheDefaultPageSize_WhenNoPagingArgumentIsGiven()
    {
        // Arrange
        const int entityCount = 25;
        await AddRangeAsync(TeamFactory.Generate(entityCount));

        // Act
        var result = await ExecuteAsync(
            """
            query { teams { totalCount nodes { id name totalPowerLevel } pageInfo { hasNextPage } } }
            """);

        // Assert
        var teams = result.Field("teams");
        teams.GetProperty("nodes").GetArrayLength().Should().Be(DefaultPageSize);
        teams.GetProperty("totalCount").GetInt32().Should().Be(entityCount);
        teams.GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean().Should().BeTrue();

        var first = teams.GetProperty("nodes")[0];
        first.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        first.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("ASC")]
    [InlineData("DESC")]
    public async Task Query_ShouldSortByAnAllowedField(string direction)
    {
        // Arrange
        await AddRangeAsync(TeamFactory.Generate(10));
        var expected = await GetQueryable<Team>()
            .OrderBy(t => t.Name)
            .Select(t => t.Name)
            .ToArrayAsync(CancellationToken);
        if (direction == "DESC")
            expected = [.. expected.Reverse()];

        // Act
        var result = await ExecuteAsync(
            $$"""
              query { teams(first: 10, order: [{ name: {{direction}} }]) { nodes { name } } }
              """);

        // Assert
        result.Field("teams").GetProperty("nodes").EnumerateArray()
            .Select(n => n.GetProperty("name").GetString())
            .Should().Equal(expected);
    }

    [Fact]
    public async Task Query_ShouldFail_WhenSortingByAFieldThatIsNotAllowed()
    {
        // Act
        var result = await ExecuteAsync("query { teams(order: [{ createdAt: ASC }]) { totalCount } }");

        // Assert
        result.HasErrors.Should().BeTrue(result.RawBody);
        result.RawBody.Should().Contain("createdAt");
    }
}
