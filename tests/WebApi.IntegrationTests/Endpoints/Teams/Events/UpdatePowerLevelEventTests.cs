using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Common.Domain.Teams;
using SSW.VerticalSliceArchitecture.Features.Heroes.Commands;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Utilities;
using System.Net;
using System.Net.Http.Json;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.Teams.Events;

public class UpdatePowerLevelEventTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Command_UpdatePowerOnTeam()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        List<Power> powers = [new Power("Strength", 10)];
        hero.UpdatePowers(powers);
        team.AddHero(hero);
        await AddAsync(team);
        powers.Add(new Power("Speed", 5));
        var powerDtos = powers.Select(p => new UpdateHeroCommand.UpdateHeroPowerDto(p.Name, p.PowerLevel));
        var cmd = new UpdateHeroCommand.Request(hero.Name, hero.Alias, powerDtos);
        cmd.HeroId = hero.Id.Value;
        var client = GetAnonymousClient();

        // Act
        var result = await client.PutAsJsonAsync($"/api/heroes/{cmd.HeroId}", cmd, CancellationToken);

        // Assert
        await Wait.ForEventualConsistency();
        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(new TeamByIdSpec(team.Id))
            .FirstOrDefaultAsync(CancellationToken);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        updatedTeam!.TotalPowerLevel.Should().Be(15);
    }
}