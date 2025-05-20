using SSW.VerticalSliceArchitecture.Common.Domain.Base.Interfaces;

namespace SSW.VerticalSliceArchitecture.Common.Domain.Base;

/// <summary>
/// Cluster of objects treated as a single unit.
/// Can contain entities, value objects, and other aggregates.
/// Enforce business rules (i.e. invariants)
/// Can be created externally.
/// Can raise domain events.
/// Represent a transactional boundary (i.e. all changes are saved or none are saved)
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        var copy = _domainEvents.ToList().AsReadOnly();
        _domainEvents.Clear();

        return copy;
    }
}