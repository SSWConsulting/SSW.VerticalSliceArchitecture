using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Heroes.Mutations;

public class UpdateHeroMutationTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Document =
        """
        mutation UpdateHero($input: UpdateHeroInput!) {
          updateHero(input: $input) {
            hero { id name alias powerLevel }
            errors { __typename ... on Error { message } }
          }
        }
        """;

    [Fact]
    public async Task Mutation_ShouldUpdateHero()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        await AddAsync(hero);

        var input = new
        {
            heroId = hero.Id.Value.ToString(),
            name = "Peter Benjamin Parker",
            alias = "The Amazing Spider-Man",
            powers = new[] { new { name = "Wall Crawling", powerLevel = 9 } }
        };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var payload = result.Field("updateHero");
        payload.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Null, result.RawBody);

        var updated = await GetQueryable<Hero>().FirstAsync(h => h.Id == hero.Id, CancellationToken);
        updated.Name.Should().Be("Peter Benjamin Parker");
        updated.Alias.Should().Be("The Amazing Spider-Man");
        updated.PowerLevel.Should().Be(9);
        updated.Powers.Should().ContainSingle();
    }

    [Fact]
    public async Task Mutation_ShouldReturnNotFound_WhenTheHeroDoesNotExist()
    {
        // Arrange
        var input = new
        {
            heroId = Guid.CreateVersion7().ToString(),
            name = "Clark Kent",
            alias = "Superman",
            powers = new[] { new { name = "Flight", powerLevel = 8 } }
        };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var errors = result.Field("updateHero").GetProperty("errors").EnumerateArray().ToList();
        errors.Should().ContainSingle();
        errors[0].GetProperty("__typename").GetString().Should().Be("NotFoundError");
        errors[0].GetProperty("message").GetString().Should().Be(HeroErrors.NotFound.Description);
    }
}
