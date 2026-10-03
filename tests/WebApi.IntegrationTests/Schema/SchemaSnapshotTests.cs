using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using SSW.VerticalSliceArchitecture.IntegrationTests.Common;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Schema;

/// <summary>
/// Compares the schema the server builds with the checked-in <c>schema.graphql</c>.
/// </summary>
/// <remarks>
/// A GraphQL schema is a published contract, and almost every change to a resolver signature
/// changes it. This test makes the change visible in the diff, so removing a field or renaming an
/// argument is a decision someone reviews rather than something a client discovers.
/// <para>
/// To accept a change, regenerate the file:
/// <c>dotnet run --project src/WebApi -- schema export --output src/WebApi/schema.graphql</c>
/// </para>
/// </remarks>
public class SchemaSnapshotTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Schema_ShouldMatchTheCheckedInSnapshot()
    {
        // Arrange
        var expected = await File.ReadAllTextAsync("schema.graphql", CancellationToken);

        // Act
        var actual = await BuildSchemaAsync();

        // Assert
        Normalise(actual).Should().Be(
            Normalise(expected),
            "the schema changed. Run: dotnet run --project src/WebApi -- schema export --output src/WebApi/schema.graphql");
    }

    private async Task<string> BuildSchemaAsync()
    {
        var provider = Services.GetRequiredService<IRequestExecutorProvider>();
        var executor = await provider.GetExecutorAsync(cancellationToken: CancellationToken);

        return executor.Schema.ToString();
    }

    private static string Normalise(string schema) =>
        schema.ReplaceLineEndings("\n").TrimEnd();
}
