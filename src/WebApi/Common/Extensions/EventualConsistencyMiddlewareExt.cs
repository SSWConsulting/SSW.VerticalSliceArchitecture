using SSW.VerticalSliceArchitecture.Common.Middleware;

namespace SSW.VerticalSliceArchitecture.Common.Extensions;

public static class EventualConsistencyMiddlewareExt
{
    public static IApplicationBuilder UseEventualConsistencyMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<EventualConsistencyMiddleware>();
        return app;
    }
}