using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.FastEndpoints;

public class LoggingPreProcessor : IGlobalPreProcessor
{
    private readonly ILogger _logger;

    public LoggingPreProcessor(ILogger<LoggingPreProcessor> logger)
    {
        _logger = logger;
    }

    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        var requestName = context.Request?.GetType().Name;
        var userId = currentUserService.UserId ?? string.Empty;

        _logger.WebApiRequest(requestName, userId, context.Request?.ToString());

        await Task.CompletedTask;
    }
}

// Compile-time logging: the source generator emits a level-checked, allocation-free
// method, which also satisfies CA1873 (the generated call is not an ILogger.Log* shape).
internal static partial class LoggingPreProcessorLog
{
    [LoggerMessage(LogLevel.Information, "WebApi Request: {Name} {@UserId} {@Request}")]
    public static partial void WebApiRequest(this ILogger logger, string? name, string userId, string? request);
}