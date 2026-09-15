using SSW.VerticalSliceArchitecture.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
