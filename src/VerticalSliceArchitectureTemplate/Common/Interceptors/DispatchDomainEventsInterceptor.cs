using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VerticalSliceArchitectureTemplate.Common.Domain.Base.Interfaces;
using VerticalSliceArchitectureTemplate.Common.Middleware;

namespace VerticalSliceArchitectureTemplate.Common.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DispatchDomainEventsInterceptor(IPublisher publisher, IHttpContextAccessor httpContextAccessor)
    {
        _publisher = publisher;
        _httpContextAccessor = httpContextAccessor;
    }

    // NOTE: There are two options for dispatching domain events:
    // Option 1. Before changes are saved to the database (SavingChanges)
    // Option 2. After changes are saved to the database (SavedChanges)
    //
    // We are using Option 2, for several reasons:
    // - Event handlers can query the DB and expect the data to be up-to-date
    // - Event handlers that fire integration events that affect other systems, will only do
    //   so if the changes are successfully saved to the database
    //
    // The downside of this is that we may have multiple calls to save changes in a single DB request.
    // This means we no longer have a single write to the DB, so may need to wrap the entire operation
    // in a transaction to ensure consistency.
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        DispatchDomainEvents(eventData.Context).ConfigureAwait(false).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public async override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEvents(DbContext? context)
    {
        if (context is null)
            return;

        var domainEvents = context.ChangeTracker.Entries<IAggregateRoot>()
            .Select(entry => entry.Entity.PopDomainEvents())
            .SelectMany(x => x)
            .ToList();

        if (domainEvents.Count is 0)
            return;

        if (IsUserWaitingOnline())
        {
            AddDomainEventsToOfflineProcessingQueue(domainEvents);
        }
        else
        {
            await PublishDomainEvents(domainEvents);
        }
    }

    private bool IsUserWaitingOnline() => _httpContextAccessor.HttpContext is not null;

    private async Task PublishDomainEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent);
    }

    private void AddDomainEventsToOfflineProcessingQueue(IEnumerable<IDomainEvent> domainEvents)
    {
        var domainEventsQueue = _httpContextAccessor.HttpContext!.Items.TryGetValue(EventualConsistencyMiddleware.DomainEventsKey, out var value) &&
                                value is Queue<IDomainEvent> existingDomainEvents
            ? existingDomainEvents
            : new Queue<IDomainEvent>();

        // Queue is processed by EventualConsistencyMiddleware
        foreach (var domainEvent in domainEvents)
            domainEventsQueue.Enqueue(domainEvent);

        _httpContextAccessor.HttpContext.Items[EventualConsistencyMiddleware.DomainEventsKey] = domainEventsQueue;
    }
}