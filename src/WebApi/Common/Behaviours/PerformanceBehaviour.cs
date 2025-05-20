using System.Diagnostics;
using MediatR;
using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Behaviours;

public static class LongRunningRequestLog
{
    private static readonly Action<ILogger, string, long, string, object, Exception?> LogAction =
        LoggerMessage.Define<string, long, string, object>(
            LogLevel.Warning,
            new EventId(3, nameof(LongRunningRequestLog)),
            "WebApi Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@Request}");

    public static void Log(ILogger logger, string name, long elapsedMilliseconds, string userId, object request) =>
        LogAction(logger, name, elapsedMilliseconds, userId, request, null);
}

public class PerformanceBehaviour<TRequest, TResponse>(
    ILogger<TRequest> logger,
    ICurrentUserService currentUserService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer = new();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next(cancellationToken);

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            var userId = currentUserService.UserId ?? string.Empty;

            LongRunningRequestLog.Log(logger, requestName, elapsedMilliseconds, userId, request);
        }

        return response;
    }
}