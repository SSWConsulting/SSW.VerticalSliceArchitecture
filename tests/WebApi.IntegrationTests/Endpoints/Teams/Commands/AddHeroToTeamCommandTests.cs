using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Features.Teams.Endpoints;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;
using System.Net;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.Teams.Commands;

public class AddHeroToTeamCommandTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Command_ShouldAddHeroToTeam()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        await AddAsync(team);
        await AddAsync(hero);
        var cmd = new AddHeroToTeamRequest(team.Id.Value, hero.Id.Value);
        var client = GetAnonymousClient();
        
        // Act
        var result = await client.POSTAsync<AddHeroToTeamEndpoint, AddHeroToTeamRequest>(cmd);

        // Assert
        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(new TeamByIdSpec(team.Id))
            .FirstOrDefaultAsync(CancellationToken);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.Heroes.Should().HaveCount(1);
        updatedTeam.Heroes.First().Id.Should().Be(hero.Id);
        updatedTeam.TotalPowerLevel.Should().Be(hero.PowerLevel);
    }
}