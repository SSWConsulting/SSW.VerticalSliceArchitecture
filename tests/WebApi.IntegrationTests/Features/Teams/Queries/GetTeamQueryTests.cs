using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Teams.Queries;

public class GetTeamQueryTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Document =
        """
        query Team($teamId: TeamId!) {
          teamById(teamId: $teamId) {
            id
            name
            heroes { id alias powers { name powerLevel } }
            missions { description status }
          }
        }
        """;

    /// <remarks>
    /// The heroes and missions fields are the DataLoaders doing their work: the resolver for the
    /// team itself loads no navigation properties.
    /// </remarks>
    [Fact]
    public async Task Query_ShouldReturnTheTeamWithItsHeroesAndMissions()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        team.AddHero(hero);
        team.ExecuteMission("Save the world");
        await AddAsync(team);

        // Act
        var result = await ExecuteAsync(Document, new { teamId = team.Id.Value.ToString() });

        // Assert
        var found = result.Field("teamById");
        found.GetProperty("name").GetString().Should().Be(team.Name);

        var heroes = found.GetProperty("heroes").EnumerateArray().ToList();
        heroes.Should().ContainSingle();
        heroes[0].GetProperty("alias").GetString().Should().Be(hero.Alias);
        heroes[0].GetProperty("powers").GetArrayLength().Should().Be(hero.Powers.Count);

        var missions = found.GetProperty("missions").EnumerateArray().ToList();
        missions.Should().ContainSingle();
        missions[0].GetProperty("status").GetString().Should().Be("IN_PROGRESS");
    }

    /// <remarks>
    /// A query asking for something that is not there is not an error in GraphQL — the field is
    /// nullable, so the answer is null and the request still succeeds.
    /// </remarks>
    [Fact]
    public async Task Query_ShouldReturnNull_WhenTheTeamDoesNotExist()
    {
        // Act
        var result = await ExecuteAsync(Document, new { teamId = Guid.CreateVersion7().ToString() });

        // Assert
        result.Field("teamById").ValueKind.Should().Be(JsonValueKind.Null, result.RawBody);
    }
}
