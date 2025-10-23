using FastEndpoints;
using SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.Teams.Queries;

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
        var result = await client.GETAsync<GetAllTeamsEndpoint, GetAllTeamsResponse>();

        // Assert
        result.Response.IsSuccessStatusCode.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Teams.Should().HaveCount(entityCount);

        var firstTeam = result.Result.Teams.First();
        firstTeam.Id.Should().NotBeEmpty();
        firstTeam.Name.Should().NotBeEmpty();
    }
}