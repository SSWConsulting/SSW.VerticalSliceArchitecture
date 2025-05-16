using VerticalSliceArchitectureTemplate.Common.Middleware;

namespace VerticalSliceArchitectureTemplate.Common.Extensions;

public static class EventualConsistencyMiddlewareExt
{
    public static IApplicationBuilder UseEventualConsistencyMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<EventualConsistencyMiddleware>();
        return app;
    }
}