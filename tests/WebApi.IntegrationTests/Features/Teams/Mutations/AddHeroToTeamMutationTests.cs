using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Teams.Mutations;

public class AddHeroToTeamMutationTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Document =
        """
        mutation AddHeroToTeam($input: AddHeroToTeamInput!) {
          addHeroToTeam(input: $input) {
            team { id name totalPowerLevel heroes { id alias } }
            errors { __typename ... on Error { message } }
          }
        }
        """;

    [Fact]
    public async Task Mutation_ShouldAddHeroToTeam()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        await AddAsync(team);
        await AddAsync(hero);

        var input = new { teamId = team.Id.Value.ToString(), heroId = hero.Id.Value.ToString() };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var payload = result.Field("addHeroToTeam");
        payload.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Null, result.RawBody);

        // The heroes field goes through the DataLoader, so this also proves the batch load runs
        // against the same transaction the mutation just committed.
        payload.GetProperty("team").GetProperty("heroes").GetArrayLength().Should().Be(1);

        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(TeamSpec.ById(team.Id))
            .FirstOrDefaultAsync(CancellationToken);

        updatedTeam.Should().NotBeNull();
        updatedTeam.Heroes.Should().HaveCount(1);
        updatedTeam.Heroes.First().Id.Should().Be(hero.Id);
        updatedTeam.TotalPowerLevel.Should().Be(hero.PowerLevel);
    }

    [Fact]
    public async Task Mutation_ShouldReturnNotFound_WhenTheTeamDoesNotExist()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        await AddAsync(hero);

        var input = new { teamId = Guid.CreateVersion7().ToString(), heroId = hero.Id.Value.ToString() };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var errors = result.Field("addHeroToTeam").GetProperty("errors").EnumerateArray().ToList();
        errors.Should().ContainSingle();
        errors[0].GetProperty("__typename").GetString().Should().Be("NotFoundError");
        errors[0].GetProperty("message").GetString().Should().Be(TeamErrors.NotFound.Description);
    }
}
