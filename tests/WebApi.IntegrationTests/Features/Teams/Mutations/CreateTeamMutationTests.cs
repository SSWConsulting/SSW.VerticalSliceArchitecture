using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Teams.Mutations;

public class CreateTeamMutationTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Document =
        """
        mutation CreateTeam($input: CreateTeamInput!) {
          createTeam(input: $input) {
            team { id name status totalPowerLevel }
            errors { __typename ... on Error { message } }
          }
        }
        """;

    [Fact]
    public async Task Mutation_ShouldCreateTeam()
    {
        // Act
        var result = await ExecuteAsync(Document, new { input = new { name = "Avengers" } });

        // Assert
        var payload = result.Field("createTeam");
        payload.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Null, result.RawBody);
        payload.GetProperty("team").GetProperty("name").GetString().Should().Be("Avengers");
        payload.GetProperty("team").GetProperty("status").GetString().Should().Be("AVAILABLE");

        var team = await GetQueryable<Team>().FirstAsync(CancellationToken);
        team.Name.Should().Be("Avengers");
        team.Status.Should().Be(TeamStatus.Available);
    }

    [Fact]
    public async Task Mutation_ShouldReturnAValidationError_WhenTheNameIsEmpty()
    {
        // Act
        var result = await ExecuteAsync(Document, new { input = new { name = "" } });

        // Assert
        var errors = result.Field("createTeam").GetProperty("errors").EnumerateArray().ToList();
        errors.Should().ContainSingle();
        errors[0].GetProperty("__typename").GetString().Should().Be("InputValidationError");

        var teams = await GetQueryable<Team>().CountAsync(CancellationToken);
        teams.Should().Be(0);
    }
}
