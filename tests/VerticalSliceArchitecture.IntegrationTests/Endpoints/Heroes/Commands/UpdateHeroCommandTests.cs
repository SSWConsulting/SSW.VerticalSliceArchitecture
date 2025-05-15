using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitectureTemplate.Common.Domain.Heroes;
using System.Net;
using System.Net.Http.Json;
using VerticalSliceArchitectureTemplate.Features.Heroes.Commands;
using VerticalSliceArchitectureTemplate.IntegrationTests.Common;
using VerticalSliceArchitectureTemplate.IntegrationTests.Common.Factories;

namespace VerticalSliceArchitectureTemplate.IntegrationTests.Endpoints.Heroes.Commands;

public class UpdateHeroCommandTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Command_ShouldUpdateHero()
    {
        // Arrange
        var heroName = "2021-01-01T00:00:00Z";
        var heroAlias = "2021-01-01T00:00:00Z-alias";
        var hero = HeroFactory.Generate();
        await AddAsync(hero);
        (string Name, int PowerLevel)[] powers =
        [
            ("Heat vision", 7),
            ("Super-strength", 10),
            ("Flight", 8),
        ];
        var cmd = new UpdateHeroCommand.Request(
            heroName,
            heroAlias,
            powers.Select(p => new UpdateHeroCommand.UpdateHeroPowerDto(p.Name, p.PowerLevel)));
        cmd.HeroId = hero.Id.Value;
        var client = GetAnonymousClient();
        var createdTimeStamp = DateTime.Now;

        // Act
        var result = await client.PutAsJsonAsync($"/api/heroes/{cmd.HeroId}", cmd, CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        Hero item = await GetQueryable<Hero>().FirstAsync(dbHero => dbHero.Id == hero.Id, CancellationToken);

        item.Should().NotBeNull();
        item.Name.Should().Be(cmd.Name);
        item.Alias.Should().Be(cmd.Alias);
        item.PowerLevel.Should().Be(25);
        item.Powers.Should().HaveCount(3);
        item.UpdatedAt.Should().NotBe(hero.CreatedAt);
        item.UpdatedAt.Should().BeCloseTo(createdTimeStamp, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Command_WhenHeroDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var heroId = HeroId.From(Guid.NewGuid());
        var cmd = new UpdateHeroCommand.Request(
            "foo",
            "bar",
            [new UpdateHeroCommand.UpdateHeroPowerDto("Heat vision", 7)]);
        cmd.HeroId = heroId.Value;
        var client = GetAnonymousClient();

        // Act
        var result = await client.PutAsJsonAsync("/heroes", cmd, CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Hero? item = await GetQueryable<Hero>().FirstOrDefaultAsync(dbHero => dbHero.Id == heroId, CancellationToken);

        item.Should().BeNull();
    }
}