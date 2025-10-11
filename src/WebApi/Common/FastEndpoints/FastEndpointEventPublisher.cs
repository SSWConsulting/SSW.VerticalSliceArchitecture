using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;
using SSW.VerticalSliceArchitecture.Common.Middleware;

namespace SSW.VerticalSliceArchitecture.Common.FastEndpoints;

/// <summary>
/// Service to queue domain events for eventual consistency processing
/// This aligns with the existing EventualConsistencyMiddleware pattern
/// Events are automatically published by the EventualConsistencyMiddleware after the response is sent
/// </summary>
public interface IFastEndpointEventPublisher
{
    void QueueDomainEvent(IDomainEvent domainEvent);
}

public class FastEndpointEventPublisher : IFastEndpointEventPublisher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FastEndpointEventPublisher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void QueueDomainEvent(IDomainEvent domainEvent)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return;

        var domainEventsQueue = httpContext.Items.TryGetValue(EventualConsistencyMiddleware.DomainEventsKey, out var value) &&
                                value is Queue<IDomainEvent> existingDomainEvents
            ? existingDomainEvents
            : new Queue<IDomainEvent>();

        domainEventsQueue.Enqueue(domainEvent);
        httpContext.Items[EventualConsistencyMiddleware.DomainEventsKey] = domainEventsQueue;
    }
}
