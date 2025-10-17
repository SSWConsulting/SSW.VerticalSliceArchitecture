using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.FastEndpoints;

public class LoggingPreProcessor : IGlobalPreProcessor
{
    private readonly ILogger _logger;
    // private readonly ICurrentUserService _currentUserService;

    public LoggingPreProcessor(ILogger<LoggingPreProcessor> logger/*, ICurrentUserService currentUserService*/)
    {
        _logger = logger;
        // _currentUserService = currentUserService;
    }

    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        var requestName = context.Request?.GetType().Name;
        var userId = currentUserService.UserId ?? string.Empty;

        _logger.LogInformation("WebApi Request: {Name} {@UserId} {@Request}",
            requestName, userId, context.Request);

        await Task.CompletedTask;
    }
}