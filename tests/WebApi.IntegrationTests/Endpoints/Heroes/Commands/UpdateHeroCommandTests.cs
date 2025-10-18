using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Features.Heroes.Endpoints;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;
using System.Net;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.Heroes.Commands;

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
        var cmd = new UpdateHeroRequest(
            heroName,
            heroAlias,
            hero.Id.Value,
            powers.Select(p => new UpdateHeroRequest.HeroPowerDto(p.Name, p.PowerLevel)));
        var client = GetAnonymousClient();
        var createdTimeStamp = DateTime.Now;

        // Act
        var result = await client.PUTAsync<UpdateHeroEndpoint, UpdateHeroRequest>(cmd);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var item = await GetQueryable<Hero>().FirstAsync(dbHero => dbHero.Id == hero.Id, CancellationToken);

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
        var cmd = new UpdateHeroRequest(
            "foo",
            "bar",
            heroId.Value,
            [new UpdateHeroRequest.HeroPowerDto("Heat vision", 7)]);
        var client = GetAnonymousClient();

        // Act
        var result = await client.PUTAsync<UpdateHeroEndpoint, UpdateHeroRequest>(cmd);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var item = await GetQueryable<Hero>().FirstOrDefaultAsync(dbHero => dbHero.Id == heroId, CancellationToken);

        item.Should().BeNull();
    }
}