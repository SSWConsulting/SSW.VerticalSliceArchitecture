using Microsoft.EntityFrameworkCore;
using SSW.VerticalSliceArchitecture.Domain.Heroes;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Heroes.Mutations;

public class CreateHeroMutationTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Document =
        """
        mutation CreateHero($input: CreateHeroInput!) {
          createHero(input: $input) {
            hero { id name alias powerLevel powers { name powerLevel } }
            errors { __typename ... on Error { message } }
          }
        }
        """;

    [Fact]
    public async Task Mutation_ShouldCreateHero()
    {
        // Arrange
        var input = new
        {
            name = "Clark Kent",
            alias = "Superman",
            powers = new[]
            {
                new { name = "Heat vision", powerLevel = 7 },
                new { name = "Super-strength", powerLevel = 10 },
                new { name = "Flight", powerLevel = 8 }
            }
        };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var payload = result.Field("createHero");
        payload.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Null, result.RawBody);
        payload.GetProperty("hero").GetProperty("powerLevel").GetInt32().Should().Be(25);

        var item = await GetQueryable<Hero>().FirstAsync(CancellationToken);

        item.Should().NotBeNull();
        item.Name.Should().Be("Clark Kent");
        item.Alias.Should().Be("Superman");
        item.PowerLevel.Should().Be(25);
        item.Powers.Should().HaveCount(3);
        item.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(10));
    }

    /// <remarks>
    /// The point of the mutation conventions: a broken rule is a typed member of the payload's error
    /// union, not a transport-level failure, so the request still succeeds and the client can branch
    /// on <c>__typename</c>.
    /// </remarks>
    [Fact]
    public async Task Mutation_ShouldReturnAValidationError_WhenTheNameIsEmpty()
    {
        // Arrange
        var input = new
        {
            name = "",
            alias = "Superman",
            powers = new[] { new { name = "Flight", powerLevel = 8 } }
        };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var payload = result.Field("createHero");
        payload.GetProperty("hero").ValueKind.Should().Be(JsonValueKind.Null, result.RawBody);

        var errors = payload.GetProperty("errors").EnumerateArray().ToList();
        errors.Should().ContainSingle();
        errors[0].GetProperty("__typename").GetString().Should().Be("InputValidationError");

        var heroes = await GetQueryable<Hero>().CountAsync(CancellationToken);
        heroes.Should().Be(0);
    }

    [Fact]
    public async Task Mutation_ShouldReturnAValidationError_WhenAPowerLevelIsOutOfRange()
    {
        // Arrange
        var input = new
        {
            name = "Clark Kent",
            alias = "Superman",
            powers = new[] { new { name = "Flight", powerLevel = 11 } }
        };

        // Act
        var result = await ExecuteAsync(Document, new { input });

        // Assert
        var errors = result.Field("createHero").GetProperty("errors").EnumerateArray().ToList();
        errors.Should().ContainSingle();
        errors[0].GetProperty("__typename").GetString().Should().Be("InputValidationError");
    }
}
