using MediatR.Pipeline;
using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Behaviours;

public static class RequestInformationLogger
{
    private static readonly Action<ILogger, string, object, object, Exception?> LogAction =
        LoggerMessage.Define<string, object, object>(
            LogLevel.Information,
            new EventId(2, nameof(RequestInformationLogger)),
            "WebApi Request: {Name} {@UserId} {@Request}");

    public static void Log(ILogger logger, string name, object userId, object request) =>
        LogAction(logger, name, userId, request, null);
}

public class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, ICurrentUserService currentUserService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUserService.UserId ?? string.Empty;

        RequestInformationLogger.Log(logger, requestName, userId, request);

        return Task.CompletedTask;
    }
}