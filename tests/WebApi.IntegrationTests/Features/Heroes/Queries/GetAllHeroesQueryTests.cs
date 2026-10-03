using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Heroes.Queries;

/// <remarks>
/// Documents are written out in full rather than built by a helper, because what these tests pin
/// down is the schema contract itself — the argument names, the connection shape, and which fields
/// a client may filter and sort by.
/// </remarks>
public class GetAllHeroesQueryTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    [Fact]
    public async Task Query_ShouldReturnTheDefaultPageSize_WhenNoPagingArgumentIsGiven()
    {
        // Arrange
        const int entityCount = 25;
        await AddRangeAsync(HeroFactory.Generate(entityCount));

        // Act
        var result = await ExecuteAsync(
            """
            query { heroes { totalCount nodes { alias } pageInfo { hasNextPage hasPreviousPage endCursor } } }
            """);

        // Assert
        var heroes = result.Field("heroes");
        heroes.GetProperty("nodes").GetArrayLength().Should().Be(DefaultPageSize);
        heroes.GetProperty("totalCount").GetInt32().Should().Be(entityCount);
        heroes.GetProperty("pageInfo").GetProperty("hasNextPage").GetBoolean().Should().BeTrue();
        heroes.GetProperty("pageInfo").GetProperty("hasPreviousPage").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Query_ShouldReturnTheNextPage_WhenGivenTheEndCursor()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(10));
        var expected = await SortedAliases();

        var firstPage = await ExecuteAsync(
            """
            query { heroes(first: 4, order: [{ alias: ASC }]) { nodes { alias } pageInfo { endCursor } } }
            """);
        var cursor = firstPage.Field("heroes").GetProperty("pageInfo").GetProperty("endCursor").GetString();

        // Act
        var result = await ExecuteAsync(
            """
            query Heroes($after: String) {
              heroes(first: 4, after: $after, order: [{ alias: ASC }]) { nodes { alias } }
            }
            """,
            new { after = cursor });

        // Assert
        Aliases(firstPage).Should().Equal(expected.Take(4));
        Aliases(result).Should().Equal(expected.Skip(4).Take(4));
    }

    [Theory]
    [InlineData("ASC")]
    [InlineData("DESC")]
    public async Task Query_ShouldSortByAnAllowedField(string direction)
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(10));
        var expected = await SortedAliases();
        if (direction == "DESC")
            expected = [.. expected.Reverse()];

        // Act
        var result = await ExecuteAsync(
            $$"""
              query { heroes(first: 10, order: [{ alias: {{direction}} }]) { nodes { alias } } }
              """);

        // Assert
        Aliases(result).Should().Equal(expected);
    }

    // A numeric field sorts through a different expression shape from a string field, so a
    // string-only suite says nothing about whether EF Core can translate the numeric one.
    [Fact]
    public async Task Query_ShouldSortByANumericField()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(10));
        var expected = await GetQueryable<Hero>()
            .OrderByDescending(h => h.PowerLevel)
            .Select(h => h.PowerLevel)
            .ToArrayAsync(CancellationToken);

        // Act
        var result = await ExecuteAsync(
            """
            query { heroes(first: 10, order: [{ powerLevel: DESC }]) { nodes { powerLevel } } }
            """);

        // Assert
        result.Field("heroes").GetProperty("nodes").EnumerateArray()
            .Select(n => n.GetProperty("powerLevel").GetInt32())
            .Should().Equal(expected);
    }

    [Fact]
    public async Task Query_ShouldFilterByAnAllowedField()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(5));
        var target = await GetQueryable<Hero>().FirstAsync(CancellationToken);

        // Act
        var result = await ExecuteAsync(
            """
            query Heroes($alias: String!) {
              heroes(where: { alias: { eq: $alias } }) { totalCount nodes { alias } }
            }
            """,
            new { alias = target.Alias });

        // Assert
        Aliases(result).Should().AllBe(target.Alias);
        Aliases(result).Should().NotBeEmpty();
    }

    /// <remarks>
    /// The allow-list in <c>HeroFilterType</c> is what makes this fail. Without it the audit columns
    /// would be filterable, and the failure would be a silent change in the public schema.
    /// </remarks>
    [Fact]
    public async Task Query_ShouldFail_WhenFilteringByAFieldThatIsNotAllowed()
    {
        // Act
        var result = await ExecuteAsync(
            """
            query { heroes(where: { createdBy: { eq: "System" } }) { totalCount } }
            """);

        // Assert
        result.HasErrors.Should().BeTrue(result.RawBody);
        result.RawBody.Should().Contain("createdBy");
    }

    [Fact]
    public async Task Query_ShouldFail_WhenAskingForMoreThanTheMaximumPageSize()
    {
        // Act
        var result = await ExecuteAsync($"query {{ heroes(first: {MaxPageSize + 1}) {{ totalCount }} }}");

        // Assert
        result.HasErrors.Should().BeTrue(result.RawBody);
    }

    [Fact]
    public async Task Query_ShouldReturnHeroPowers()
    {
        // Arrange
        await AddRangeAsync(HeroFactory.Generate(1));

        // Act
        var result = await ExecuteAsync(
            """
            query { heroes { nodes { id name alias powerLevel powers { name powerLevel } } } }
            """);

        // Assert
        var nodes = result.Field("heroes").GetProperty("nodes").EnumerateArray().ToList();
        var hero = nodes.Should().ContainSingle().Subject;
        hero.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        hero.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
        hero.GetProperty("powers").GetArrayLength().Should().BeGreaterThan(0);
    }

    private static IEnumerable<string?> Aliases(GraphQlResult result) =>
        result.Field("heroes").GetProperty("nodes").EnumerateArray()
            .Select(n => n.GetProperty("alias").GetString());

    /// <remarks>
    /// The expected order comes out of the database rather than a hard-coded list, because the
    /// heroes are randomly generated.
    /// </remarks>
    private async Task<string[]> SortedAliases() =>
        await GetQueryable<Hero>()
            .OrderBy(h => h.Alias)
            .Select(h => h.Alias)
            .ToArrayAsync(CancellationToken);
}
