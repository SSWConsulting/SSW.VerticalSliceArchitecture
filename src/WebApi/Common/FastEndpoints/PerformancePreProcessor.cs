using System.Diagnostics;

namespace SSW.VerticalSliceArchitecture.Common.FastEndpoints;

public class PerformancePreProcessor : IGlobalPreProcessor
{
    private const string ActivityKey = "PerformanceStopwatch";
    
    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        context.HttpContext.Items[ActivityKey] = stopwatch;
        await Task.CompletedTask;
    }
}