namespace SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;

public interface IAggregateRoot
{
    void AddDomainEvent(IEvent domainEvent);

    IReadOnlyList<IEvent> PopDomainEvents();
}