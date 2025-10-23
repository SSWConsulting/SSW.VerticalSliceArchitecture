using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Common.Domain.Heroes;
using SSW.VerticalSliceArchitecture.Features.Heroes.Endpoints;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using System.Net;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Endpoints.Heroes.Commands;

public class CreateHeroCommandTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Command_ShouldCreateHero()
    {
        // Arrange
        (string Name, int PowerLevel)[] powers =
        [
            ("Heat vision", 7),
            ("Super-strength", 10),
            ("Flight", 8),
        ];
        var cmd = new CreateHeroRequest(
            "Clark Kent",
            "Superman",
            powers.Select(p => new CreateHeroRequest.HeroPowerDto(p.Name, p.PowerLevel)));
        var client = GetAnonymousClient();

        // Act
        var result = await client.POSTAsync<CreateHeroEndpoint, CreateHeroRequest, CreateHeroResponse>(cmd);

        // Assert
        result.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await GetQueryable<Hero>().FirstAsync(CancellationToken);

        item.Should().NotBeNull();
        item.Name.Should().Be(cmd.Name);
        item.Alias.Should().Be(cmd.Alias);
        item.PowerLevel.Should().Be(25);
        item.Powers.Should().HaveCount(3);
        item.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(10));
    }
}