using System.Net.Http.Json;
using VerticalSliceArchitectureTemplate.Features.Teams.Queries;
using VerticalSliceArchitectureTemplate.IntegrationTests.Common;
using VerticalSliceArchitectureTemplate.IntegrationTests.Common.Factories;

namespace VerticalSliceArchitectureTemplate.IntegrationTests.Endpoints.Teams.Queries;

public class GetAllTeamsQueryTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Query_ShouldReturnAllTeams()
    {
        // Arrange
        const int entityCount = 10;
        var entities = TeamFactory.Generate(entityCount);
        await AddRangeAsync(entities);
        var client = GetAnonymousClient();

        // Act
        var result = await client.GetFromJsonAsync<GetAllTeamsQuery.TeamDto[]>("/api/teams", CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Length.Should().Be(entityCount);

        var firstTeam = result.First();
        firstTeam.Id.Should().NotBeEmpty();
        firstTeam.Name.Should().NotBeEmpty();
    }
}