namespace VerticalSliceArchitectureTemplate.Common.Extensions;

public static class WebApplicationExt
{
    /// <summary>
    /// Adds an 'api' prefix to the route, and adds the group name as a tag and enables OpenAPI.
    /// </summary>
    public static RouteGroupBuilder MapApiGroup(this IEndpointRouteBuilder endpoints, string groupName)
    {
        var group = endpoints
            .MapGroup($"api/{groupName.ToLowerInvariant()}")
            .WithTags(groupName)
            .WithOpenApi();

        return group;
    }
}