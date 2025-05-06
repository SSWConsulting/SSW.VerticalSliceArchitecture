using MediatR.Pipeline;
using VerticalSliceArchitectureTemplate.Common.Services;

namespace VerticalSliceArchitectureTemplate.Common.Behaviours;

public class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, CurrentUserService currentUserService)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUserService.UserId ?? string.Empty;

        logger.LogInformation("CleanArchitecture Request: {Name} {@UserId} {@Request}",
            requestName, userId, request);

        return Task.CompletedTask;
    }
}