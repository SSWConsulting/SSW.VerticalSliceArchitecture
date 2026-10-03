using System.Collections.Concurrent;
using System.Reflection;
using SSW.VerticalSliceArchitecture.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Events;

/// <summary>
/// Resolves every <see cref="IDomainEventHandler{TEvent}"/> registered for an event and runs them
/// in turn, each in its own dependency injection scope.
/// </summary>
/// <remarks>
/// A scope per handler keeps a handler's <c>ApplicationDbContext</c> separate from the one whose
/// <c>SaveChanges</c> raised the event. Sharing it would mean a handler saving a half-finished
/// change tracker, and re-entering the interceptor that dispatched the event.
/// <para>
/// The reflection is the price of a dispatcher with no messaging library behind it: the event is
/// only known as <see cref="IDomainEvent"/> at this point, so the closed handler interface has to
/// be built at run time. The result is cached per event type.
/// </para>
/// </remarks>
public sealed class DomainEventDispatcher(IServiceScopeFactory scopeFactory) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, HandlerInvoker> Invokers = new();

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ThrowIfNull(domainEvent);

        var invoker = Invokers.GetOrAdd(domainEvent.GetType(), HandlerInvoker.For);

        await using var scope = scopeFactory.CreateAsyncScope();

        foreach (var handler in invoker.ResolveHandlers(scope.ServiceProvider))
            await invoker.InvokeAsync(handler, domainEvent, cancellationToken);
    }

    /// <summary>
    /// The closed handler type for one event type, plus its <c>HandleAsync</c> method.
    /// </summary>
    private sealed class HandlerInvoker
    {
        private readonly Type _handlerType;
        private readonly MethodInfo _handleAsync;

        private HandlerInvoker(Type handlerType, MethodInfo handleAsync)
        {
            _handlerType = handlerType;
            _handleAsync = handleAsync;
        }

        public static HandlerInvoker For(Type eventType)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

            var handleAsync = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))
                              ?? throw new InvalidOperationException(
                                  $"{handlerType} does not declare {nameof(IDomainEventHandler<IDomainEvent>.HandleAsync)}.");

            return new HandlerInvoker(handlerType, handleAsync);
        }

        public IEnumerable<object> ResolveHandlers(IServiceProvider services) =>
            services.GetServices(_handlerType).OfType<object>();

        public Task InvokeAsync(object handler, IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            // The method is non-void and returns Task, so a null result means the handler type no
            // longer matches the interface this invoker was built from — a bug, not a no-op.
            return _handleAsync.Invoke(handler, [domainEvent, cancellationToken]) as Task
                   ?? throw new InvalidOperationException(
                       $"{handler.GetType()}.{nameof(IDomainEventHandler<IDomainEvent>.HandleAsync)} did not return a {nameof(Task)}.");
        }
    }
}
