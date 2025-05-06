namespace SSW.CleanArchitecture.WebApi.Extensions;

public static class WebApplicationExt
{
    /// <summary>
    /// Adds an 'api' prefix to the route, and adds the group name as a tag and enables OpenAPI.
    /// </summary>
    public static RouteGroupBuilder MapApiGroup(this WebApplication app, string groupName)
    {
        var group = app
            .MapGroup($"api/{groupName}")
            .WithTags(groupName)
            .WithOpenApi();

        return group;
    }
}