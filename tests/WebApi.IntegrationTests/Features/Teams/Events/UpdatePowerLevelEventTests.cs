using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Domain.Teams;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Utilities;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Teams.Events;

/// <remarks>
/// The mutation raises <c>PowerLevelUpdatedEvent</c>, the middleware publishes it after the
/// response, and the Teams handler recalculates the total. Nothing in the mutation's own slice
/// knows any of that happens.
/// </remarks>
public class UpdatePowerLevelEventTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Mutation_ShouldUpdateThePowerLevelOfTheTeam()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        List<Power> powers = [new Power("Strength", 10)];
        hero.UpdatePowers(powers);
        team.AddHero(hero);
        await AddAsync(team);
        powers.Add(new Power("Speed", 5));

        var input = new
        {
            heroId = hero.Id.Value.ToString(),
            name = hero.Name,
            alias = hero.Alias,
            powers = powers.Select(p => new { name = p.Name, powerLevel = p.PowerLevel }).ToArray()
        };

        // Act
        var result = await ExecuteAsync(
            """
            mutation UpdateHero($input: UpdateHeroInput!) {
              updateHero(input: $input) {
                hero { id powerLevel }
                errors { __typename }
              }
            }
            """,
            new { input });

        // Assert
        result.Field("updateHero").GetProperty("errors").ValueKind.Should()
            .Be(JsonValueKind.Null, result.RawBody);

        await Wait.ForEventualConsistency();

        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(TeamSpec.ById(team.Id))
            .FirstOrDefaultAsync(CancellationToken);

        updatedTeam.Should().NotBeNull();
        updatedTeam.TotalPowerLevel.Should().Be(15);
    }
}
