using System.Text.Json;

namespace SSW.VerticalSliceArchitecture.IntegrationTests.Common.Infrastructure.GraphQL;

/// <summary>
/// One GraphQL response: the <c>data</c> object, the request-level <c>errors</c>, and the raw body
/// for failure messages.
/// </summary>
/// <remarks>
/// GraphQL answers with HTTP 200 and an <c>errors</c> array, so a test that only checked the status
/// code would pass on a failed query. Every accessor here reports the body instead.
/// </remarks>
public sealed class GraphQlResult
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly JsonElement _root;

    private GraphQlResult(JsonElement root, string rawBody)
    {
        _root = root;
        RawBody = rawBody;
    }

    public string RawBody { get; }

    public IReadOnlyList<string> Errors =>
        _root.TryGetProperty("errors", out var errors) && errors.ValueKind is JsonValueKind.Array
            ? [.. errors.EnumerateArray().Select(e => e.GetProperty("message").GetString() ?? string.Empty)]
            : [];

    public bool HasErrors => Errors.Count > 0;

    public static GraphQlResult Parse(string body)
    {
        // Cloned so the JsonDocument can be released here rather than living as long as the result.
        using var document = JsonDocument.Parse(body);
        return new GraphQlResult(document.RootElement.Clone(), body);
    }

    /// <summary>One field of the response's <c>data</c> object.</summary>
    public JsonElement Field(string name)
    {
        Errors.Should().BeEmpty("the request returned errors: {0}", RawBody);

        _root.TryGetProperty("data", out var data).Should().BeTrue("the response has no data: {0}", RawBody);

        data.TryGetProperty(name, out var field).Should()
            .BeTrue("the response data has no '{0}' field: {1}", name, RawBody);

        return field;
    }

    /// <summary>One field of the response's <c>data</c> object, deserialised.</summary>
    public T Field<T>(string name)
    {
        var value = Field(name).Deserialize<T>(Json);
        return value.Should().NotBeNull("'{0}' deserialised to null: {1}", name, RawBody).And.Subject.As<T>();
    }
}
