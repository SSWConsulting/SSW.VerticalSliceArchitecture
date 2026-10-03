using System.Net.Http.Json;
using System.Text.Json;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Common.Infrastructure.GraphQL;

/// <summary>
/// Posts GraphQL documents to <c>/graphql</c> over the test server's HttpClient.
/// </summary>
/// <remarks>
/// Deliberately thin, and deliberately not generated. The tests send the same document text a
/// client would, so a field renamed in the schema fails a test instead of passing against a
/// regenerated wrapper.
/// </remarks>
public sealed class GraphQlClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<GraphQlResult> ExecuteAsync(
        string document,
        object? variables = null,
        CancellationToken cancellationToken = default)
    {
        var request = new GraphQlRequest(document, variables);

        using var response = await httpClient.PostAsJsonAsync("/graphql", request, Json, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        return GraphQlResult.Parse(body);
    }

    private sealed record GraphQlRequest(string Query, object? Variables);
}
