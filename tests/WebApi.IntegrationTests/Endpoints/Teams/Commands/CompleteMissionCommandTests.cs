using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;
using System.Net;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.Teams.Commands;

public class CompleteMissionCommandTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Command_ShouldCompleteMission()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        team.AddHero(hero);
        team.ExecuteMission("Save the world");
        await AddAsync(team);
        var cmd = new CompleteMissionRequest(team.Id.Value);
        var client = GetAnonymousClient();

        // Act
        var result = await client.POSTAsync<CompleteMissionEndpoint, CompleteMissionRequest>(cmd);

        // Assert
        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(new TeamByIdSpec(team.Id))
            .FirstOrDefaultAsync(CancellationToken);
        var mission = updatedTeam!.Missions.First();

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        updatedTeam!.Missions.Should().HaveCount(1);
        updatedTeam.Status.Should().Be(TeamStatus.Available);
        mission.Status.Should().Be(MissionStatus.Complete);
    }
}