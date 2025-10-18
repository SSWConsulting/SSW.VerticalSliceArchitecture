using SSW.VerticalSliceArchitecture.Common.Domain.Base.EventualConsistency;

namespace SSW.VerticalSliceArchitecture.Common.Middleware;

public class EventualConsistencyMiddleware
{
    public const string DomainEventsKey = "DomainEventsKey";

    private readonly RequestDelegate _next;

    public EventualConsistencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        context.Response.OnCompleted(async () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteInTransactionAsync(async () =>
            {
                await PublishEvents(context);
            }, null!);
        });

        await _next(context);
    }

    private static async Task PublishEvents(HttpContext context)
    {
        try
        {
            if (context.Items.TryGetValue(DomainEventsKey, out var value) &&
                value is Queue<IEvent> domainEvents)
            {
                while (domainEvents.TryDequeue(out var nextEvent))
                    await nextEvent.PublishAsync();
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