using SSW.VerticalSliceArchitecture.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Events;

/// <summary>
/// Handles one kind of domain event. Implementations live in the slice that reacts to the event,
/// not in the slice that raises it.
/// </summary>
/// <remarks>
/// Every implementation found in the assembly is registered and run, so one event can have many
/// handlers. A handler that throws stops the ones after it — see <see cref="DomainEventDispatcher"/>.
/// </remarks>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken);
}
