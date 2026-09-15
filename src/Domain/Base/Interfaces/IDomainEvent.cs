namespace SSW.VerticalSliceArchitecture.Domain.Base.Interfaces;

/// <summary>
/// Marker interface for something that happened in the domain, raised by an aggregate and
/// published after the change is saved.
/// </summary>
/// <remarks>
/// Declared here, rather than taken from a messaging library, so that the domain stays free of
/// framework packages. The handler contract and the dispatcher that runs it live in the API
/// project, which is the layer allowed to know about dependency injection.
/// </remarks>
public interface IDomainEvent;
