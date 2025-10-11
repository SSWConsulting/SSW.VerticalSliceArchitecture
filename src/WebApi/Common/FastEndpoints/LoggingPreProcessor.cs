using FastEndpoints;
using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.FastEndpoints;

public class LoggingPreProcessor : IGlobalPreProcessor
{
    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService(typeof(ILogger<>).MakeGenericType(context.Request.GetType())) as ILogger;
        var currentUserService = context.HttpContext.Resolve<ICurrentUserService>();
        
        var requestName = context.Request.GetType().Name;
        var userId = currentUserService.UserId ?? string.Empty;

        logger?.LogInformation("WebApi Request: {Name} {@UserId} {@Request}",
            requestName, userId, context.Request);

        await Task.CompletedTask;
    }
}
