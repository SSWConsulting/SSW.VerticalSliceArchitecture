using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Interfaces;
using System.Diagnostics;

namespace SSW.VerticalSliceArchitecture.Common.FastEndpoints;

public class PerformancePostProcessor : IGlobalPostProcessor
{
    private const string ActivityKey = "PerformanceStopwatch";
    
    public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        if (context.HttpContext.Items.TryGetValue(ActivityKey, out var stopwatchObj) 
            && stopwatchObj is Stopwatch stopwatch)
        {
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService(typeof(ILogger<>).MakeGenericType(context.Request.GetType())) as ILogger;
                var currentUserService = context.HttpContext.Resolve<ICurrentUserService>();
                
                var requestName = context.Request.GetType().Name;
                var userId = currentUserService.UserId ?? string.Empty;

                logger?.LogWarning(
                    "WebApi Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
                    requestName, elapsedMilliseconds, userId, context.Request);
            }
        }

        await Task.CompletedTask;
    }
}
