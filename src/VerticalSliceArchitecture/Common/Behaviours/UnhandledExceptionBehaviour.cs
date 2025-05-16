using MediatR;

namespace SSW.VerticalSliceArchitecture.Common.Behaviours;

public static class UnhandledExceptionBehaviourLogger
{
    private static readonly Action<ILogger, string, object, Exception> LogAction =
        LoggerMessage.Define<string, object>(
            LogLevel.Error,
            new EventId(1, nameof(UnhandledExceptionBehaviourLogger)),
            "VerticalSliceArchitecture Request: Unhandled Exception for Request {Name} {@Request}");

    public static void Log(ILogger logger, string name, object request, Exception exception) =>
        LogAction(logger, name, request, exception);
}

public class UnhandledExceptionBehaviour<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            UnhandledExceptionBehaviourLogger.Log(logger, requestName, request, ex);

            throw;
        }
    }
}