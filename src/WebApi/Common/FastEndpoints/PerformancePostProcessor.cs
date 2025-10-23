using SSW.VerticalSliceArchitecture.Common.Interfaces;
using System.Diagnostics;

namespace SSW.VerticalSliceArchitecture.Common.FastEndpoints;

public class PerformancePostProcessor : IGlobalPostProcessor
{
    private const string ActivityKey = "PerformanceStopwatch";

    private readonly ILogger _logger;

    public PerformancePostProcessor(ILogger<PerformancePostProcessor> logger)
    {
        _logger = logger;
    }

    public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        if (context.HttpContext.Items.TryGetValue(ActivityKey, out var stopwatchObj) 
            && stopwatchObj is Stopwatch stopwatch)
        {
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = context.Request?.GetType().Name;
                var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
                var userId = currentUserService.UserId ?? string.Empty;

                _logger?.LogWarning(
                    "WebApi Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}",
                    requestName, elapsedMilliseconds, userId, context.Request);
            }
        }

        await Task.CompletedTask;
    }
}