using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Teams.Mutations;

public class CompleteMissionMutationTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Document =
        """
        mutation CompleteMission($input: CompleteMissionInput!) {
          completeMission(input: $input) {
            team { id status missions { description status } }
            errors { __typename ... on Error { message } }
          }
        }
        """;

    [Fact]
    public async Task Mutation_ShouldCompleteMission()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        team.AddHero(hero);
        team.ExecuteMission("Save the world");
        await AddAsync(team);

        // Act
        var result = await ExecuteAsync(Document, new { input = new { teamId = team.Id.Value.ToString() } });

        // Assert
        var payload = result.Field("completeMission");
        payload.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Null, result.RawBody);
        payload.GetProperty("team").GetProperty("status").GetString().Should().Be("AVAILABLE");

        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(TeamSpec.ById(team.Id))
            .FirstOrDefaultAsync(CancellationToken);

        updatedTeam.Should().NotBeNull();
        updatedTeam.Missions.Should().HaveCount(1);
        updatedTeam.Status.Should().Be(TeamStatus.Available);
        updatedTeam.Missions.First().Status.Should().Be(MissionStatus.Complete);
    }

    [Fact]
    public async Task Mutation_ShouldReturnAConflict_WhenTheTeamIsNotOnAMission()
    {
        // Arrange
        var team = TeamFactory.Generate();
        await AddAsync(team);

        // Act
        var result = await ExecuteAsync(Document, new { input = new { teamId = team.Id.Value.ToString() } });

        // Assert
        var errors = result.Field("completeMission").GetProperty("errors").EnumerateArray().ToList();
        errors.Should().ContainSingle();
        errors[0].GetProperty("__typename").GetString().Should().Be("ConflictError");
        errors[0].GetProperty("message").GetString().Should().Be(TeamErrors.NotOnMission.Description);
    }
}
