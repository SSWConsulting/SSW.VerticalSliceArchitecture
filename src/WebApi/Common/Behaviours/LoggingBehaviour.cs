using MediatR.Pipeline;
using SSW.VerticalSliceArchitecture.Common.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Behaviours;

public class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, ICurrentUserService currentUserService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUserService.UserId ?? string.Empty;

        logger.LogInformation("WebApi Request: {Name} {@UserId} {@Request}",
            requestName, userId, request);

        return Task.CompletedTask;
    }
}