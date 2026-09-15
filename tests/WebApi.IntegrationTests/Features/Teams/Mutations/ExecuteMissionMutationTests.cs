using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Teams.Mutations;

public class ExecuteMissionMutationTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Document =
        """
        mutation ExecuteMission($input: ExecuteMissionInput!) {
          executeMission(input: $input) {
            team { id status missions { description status } }
            errors { __typename ... on Error { message } }
          }
        }
        """;

    [Fact]
    public async Task Mutation_ShouldExecuteMission()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        team.AddHero(hero);
        await AddAsync(team);

        var input = new { teamId = team.Id.Value.ToString(), description = "Save the world" };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var payload = result.Field("executeMission");
        payload.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Null, result.RawBody);
        payload.GetProperty("team").GetProperty("status").GetString().Should().Be("ON_MISSION");

        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(TeamSpec.ById(team.Id))
            .FirstOrDefaultAsync(CancellationToken);

        updatedTeam.Should().NotBeNull();
        updatedTeam.Missions.Should().HaveCount(1);
        updatedTeam.Status.Should().Be(TeamStatus.OnMission);
        updatedTeam.Missions.First().Status.Should().Be(MissionStatus.InProgress);
    }

    /// <remarks>
    /// The domain returns <c>TeamErrors.NoHeroes</c> as a conflict, and the slice's
    /// <c>ThrowOnError</c> is what turns it into this union member.
    /// </remarks>
    [Fact]
    public async Task Mutation_ShouldReturnAConflict_WhenTheTeamHasNoHeroes()
    {
        // Arrange
        var team = TeamFactory.Generate();
        await AddAsync(team);

        var input = new { teamId = team.Id.Value.ToString(), description = "Save the world" };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var errors = result.Field("executeMission").GetProperty("errors").EnumerateArray().ToList();
        errors.Should().ContainSingle();
        errors[0].GetProperty("__typename").GetString().Should().Be("ConflictError");
        errors[0].GetProperty("message").GetString().Should().Be(TeamErrors.NoHeroes.Description);
    }
}
