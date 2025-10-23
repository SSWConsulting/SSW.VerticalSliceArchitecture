using SSW.VerticalSliceArchitecture.Common.FastEndpoints;

namespace SSW.VerticalSliceArchitecture.Host.Extensions;

public static class FastEndpointsExt
{
    public static IApplicationBuilder UseCustomFastEndpoints(this IApplicationBuilder app)
    {
        app.UseFastEndpoints(config =>
        {
            config.Endpoints.RoutePrefix = "api";
            config.Endpoints.Configurator = ep =>
            {
                // Add global pre-processors
                ep.PreProcessor<LoggingPreProcessor>(Order.Before);
                ep.PreProcessor<PerformancePreProcessor>(Order.Before);

                // Add global post-processors
                ep.PostProcessor<PerformancePostProcessor>(Order.After);

                // TODO: Configure your authentication provider here
                ep.AllowAnonymous();
            };

            config.Errors.UseProblemDetails();
        });

        return app;
    }
}