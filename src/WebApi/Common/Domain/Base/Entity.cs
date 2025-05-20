namespace SSW.VerticalSliceArchitecture.Common.Domain.Base;

/// <summary>
/// Entities have an ID and a lifecycle (i.e. created, modified, and deleted)
/// They can be created within the domain, but not externally.
/// Enforce business rules (i.e. invariants)
/// </summary>
public abstract class Entity<TId> : Auditable
{
    public TId Id { get; set; } = default!;
}