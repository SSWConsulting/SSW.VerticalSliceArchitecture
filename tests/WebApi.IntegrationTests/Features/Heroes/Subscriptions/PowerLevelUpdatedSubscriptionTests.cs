using System.Buffers;
using System.Text;
using HotChocolate.Execution;
using HotChocolate.Transport.Formatters;
using Microsoft.Extensions.DependencyInjection;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common.Factories;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Features.Heroes.Subscriptions;

/// <remarks>
/// Subscribes in process rather than over a websocket: the point of the test is the chain from a
/// mutation through the domain event to the topic, and a websocket client would only add a
/// transport to the part that already works. The mutation still goes over HTTP, because that is
/// what makes the eventual consistency middleware publish the event.
/// </remarks>
public class PowerLevelUpdatedSubscriptionTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Subscription_ShouldPushTheHero_WhenTheirPowersChange()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        await AddAsync(hero);

        var provider = Services.GetRequiredService<IRequestExecutorProvider>();
        var executor = await provider.GetExecutorAsync(cancellationToken: CancellationToken);

        var subscription = await executor.ExecuteAsync(
            "subscription { heroPowerLevelUpdated { id alias powerLevel } }",
            CancellationToken);

        await using var stream = subscription.ExpectResponseStream();
        await using var messages = stream.ReadResultsAsync().GetAsyncEnumerator(CancellationToken);

        // Act
        var mutation = await ExecuteAsync(
            """
            mutation UpdateHero($input: UpdateHeroInput!) {
              updateHero(input: $input) { hero { id } errors { __typename } }
            }
            """,
            new
            {
                input = new
                {
                    heroId = hero.Id.Value.ToString(),
                    name = hero.Name,
                    alias = hero.Alias,
                    powers = new[] { new { name = "Telepathy", powerLevel = 6 } }
                }
            });

        // Assert
        mutation.Field("updateHero").GetProperty("errors").ValueKind.Should()
            .Be(JsonValueKind.Null, mutation.RawBody);

        var received = await messages.MoveNextAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30), CancellationToken);
        received.Should().BeTrue("the mutation should have pushed a message onto the topic");

        var operationResult = messages.Current.ExpectOperationResult();
        operationResult.Errors.Should().BeNullOrEmpty();

        // Written out rather than read field by field off IOperationResult.Data, so the assertions
        // run against the same JSON a websocket client would receive.
        var buffer = new ArrayBufferWriter<byte>();
        new JsonResultFormatter().Format(operationResult, buffer);

        var pushed = GraphQlResult.Parse(Encoding.UTF8.GetString(buffer.WrittenSpan));
        var payload = pushed.Field("heroPowerLevelUpdated");

        payload.GetProperty("alias").GetString().Should().Be(hero.Alias);
        payload.GetProperty("powerLevel").GetInt32().Should().Be(6);
    }
}
