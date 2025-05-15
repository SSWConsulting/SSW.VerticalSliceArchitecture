using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitectureTemplate.Common.Domain.Teams;
using System.Net;
using System.Net.Http.Json;
using VerticalSliceArchitectureTemplate.Features.Teams.Commands;
using VerticalSliceArchitectureTemplate.IntegrationTests.Common;
using VerticalSliceArchitectureTemplate.IntegrationTests.Common.Factories;

namespace VerticalSliceArchitectureTemplate.IntegrationTests.Endpoints.Teams.Commands;

public class ExecuteMissionCommandTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Command_ShouldExecuteMission()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        team.AddHero(hero);
        await AddAsync(team);
        var teamId = team.Id.Value;
        var client = GetAnonymousClient();
        var request = new ExecuteMissionCommand.Request("Save the world");

        // Act
        var result = await client.PostAsJsonAsync($"/api/teams/{teamId}/execute-mission", request, CancellationToken);

        // Assert
        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(new TeamByIdSpec(team.Id))
            .FirstOrDefaultAsync(CancellationToken);
        var mission = updatedTeam!.Missions.First();

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedTeam!.Missions.Should().HaveCount(1);
        updatedTeam.Status.Should().Be(TeamStatus.OnMission);
        mission.Status.Should().Be(MissionStatus.InProgress);
    }
}