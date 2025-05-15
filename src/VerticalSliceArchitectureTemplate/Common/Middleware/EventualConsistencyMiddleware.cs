using MediatR;
using VerticalSliceArchitectureTemplate.Common.Domain.Base.EventualConsistency;
using VerticalSliceArchitectureTemplate.Common.Domain.Base.Interfaces;

namespace VerticalSliceArchitectureTemplate.Common.Middleware;

public class EventualConsistencyMiddleware
{
    public const string DomainEventsKey = "DomainEventsKey";

    private readonly RequestDelegate _next;

    public EventualConsistencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPublisher publisher, ApplicationDbContext dbContext)
    {
        context.Response.OnCompleted(async () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteInTransactionAsync(async () =>
            {
                await PublishEvents(context, publisher);
            }, null!);
        });

        await _next(context);
    }

    private static async Task PublishEvents(HttpContext context, IPublisher publisher)
    {
        try
        {
            if (context.Items.TryGetValue(DomainEventsKey, out var value) &&
                value is Queue<IDomainEvent> domainEvents)
            {
                while (domainEvents.TryDequeue(out var nextEvent))
                    await publisher.Publish(nextEvent);
            }
        }
        // ReSharper disable once RedundantCatchClause
        catch (EventualConsistencyException)
        {
            // TODO: handle eventual consistency exception
            throw;
        }
    }
}